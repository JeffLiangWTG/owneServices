using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportDescriptionFormatter
	{
		public ImportDescriptionFormatter(BusinessObjectFactory factory, ZString tariffNum)
		{
			fTariffNum = tariffNum;
			this.factory = factory;
		}

		public ZString Description
		{
			get
			{
				if (fDescription == "")
				{
					fDescription = BuildDescription();
				}
				return fDescription;
			}
		}

		#region Implementation

		ZString fDescription;
		readonly ZString fTariffNum;

		const int MinValidDescriptionLength = 15;
		const string OtherString = "OTHER";

		protected BusinessObjectFactory factory;

		public void GenerateFetchHint()
		{
			if (fTariffNum.Length >= 4)
			{
				List<string> partialTariffs = new List<string>();
				foreach (int i in allowablePartialTariffLengths)
				{
					if (i <= fTariffNum.Length)
					{
						factory.AddFetchHint(AUCClassSchema.UJ_Code, fTariffNum.SubstringSafe(0, i));
					}
				}
			}
		}

		protected string BuildDescription()
		{
			StringBuilder result = new StringBuilder(80);
			AUCClass[] tariffDescriptions = GetTariffDescriptions();
			if (tariffDescriptions.Length > 0)
			{
				if (IsValidDescription(tariffDescriptions[0].UJ_Txt))
				{
					result.Append(tariffDescriptions[0].UJ_Txt);
				}
				else
				{
					for (int i = 0; i < tariffDescriptions.Length; i++)
					{
						AUCClass tariffDescription = tariffDescriptions[i];
						AUCClass parentClass = tariffDescription.ParentClass;
						while (parentClass != null && tariffDescription.UJ_Code == parentClass.UJ_Code)
						{
							result.Insert(0, CleanDescription(parentClass.UJ_Txt) + " ");
							parentClass = parentClass.ParentClass;
						}
						if (IsValidDescription(tariffDescription.UJ_Txt))
						{
							result.Insert(0, CleanDescription(tariffDescription.UJ_Txt) + " ");
							break;
						}
						else
						{
							if (IsOther(tariffDescription.UJ_Txt))
							{
								result.Insert(0, GenerateOtherClause(tariffDescription));
							}
						}
					}
				}
			}
			ZString resultAsString = result.ToString().ToUpper();

			return resultAsString.SubstringSafe(0, 80);
		}

		static readonly int[] allowablePartialTariffLengths = { 4, 6, 7, 9, 10, 13 };

		protected AUCClass[] GetTariffDescriptions()
		{
			StringCollection result = new StringCollection();
			if (fTariffNum.Length >= 4)
			{
				List<string> partialTariffs = new List<string>();
				foreach (int i in allowablePartialTariffLengths)
				{
					if (i <= fTariffNum.Length)
					{
						partialTariffs.Add(fTariffNum.SubstringSafe(0, i));
					}
				}
				ZQuery tariffFilter = new ZQuery(AUCClassSchema.UJ_Code, partialTariffs);
				tariffFilter.OrderBy = AUCClass.Schema.UJ_Code + " DESC";
				return factory.Load<AUCClass>(tariffFilter);
			}
			else
			{
				return System.Array.Empty<AUCClass>();
			}
		}

		protected ZString GenerateOtherClause(AUCClass @class)
		{
			StringBuilder result = new StringBuilder();
			ZQuery siblingTariffFilter = new ZQuery(AUCClassSchema.UJ_UJ, @class.UJ_UJ);
			siblingTariffFilter.AddToFilter(new ZQuery(AUCClassSchema.PK, SQLComparisonOperator.NotEqual, @class.PK));
			siblingTariffFilter.OrderBy = AUCClass.Schema.UJ_Code;

			AUCClass[] siblingClasses = factory.Load<AUCClass>(siblingTariffFilter);
			foreach (AUCClass sibling in siblingClasses)
			{
				if (result.Length > 0)
				{
					result.Append(",");
				}
				result.Append(AbriviateOtherDescription(sibling.UJ_Txt));
			}

			return new ZString("EXCL " + result.ToString());
		}

		protected ZString CleanDescription(ZString input)
		{
			ZString result = input.Replace(":", "");
			return result;
		}

		protected ZString AbriviateOtherDescription(ZString input)
		{
			ZString result = input;
			int cutIndex = input.IndexOfAny(new char[] { ':', '(' });
			int notMoreThanIndex = input.ToUpper().IndexOf(" NOT MORE THAN");
			if (notMoreThanIndex > 0 && notMoreThanIndex < cutIndex)
			{
				cutIndex = notMoreThanIndex;
			}

			int orMoreIndex = input.ToUpper().IndexOf(" OR MORE");
			if (orMoreIndex > 0 && orMoreIndex < cutIndex)
			{
				cutIndex = orMoreIndex;
			}

			if (cutIndex > 0)
			{
				result = result.SubstringSafe(0, cutIndex).TrimEnd(' ');
			}
			return result;
		}

		protected bool IsValidDescription(ZString description)
		{
			return (description.Length > MinValidDescriptionLength) || !IsOther(description);
		}

		protected bool IsOther(ZString description)
		{
			return description.ToUpper().Contains(OtherString);
		}

		#endregion
	}
}
