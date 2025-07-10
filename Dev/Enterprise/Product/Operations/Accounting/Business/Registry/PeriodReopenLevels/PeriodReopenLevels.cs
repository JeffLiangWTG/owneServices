using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class PeriodReopenLevels : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Days = "Days";
			public const string AuthorisationRequirement = "AuthorisationRequirement";
			public const string Range = "Range";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PeriodReopenLevels();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateDays();
			ValidateAuthorisationRequirement();
			ValidateRange();
		}

		PeriodReopenLevelsCollection ParentCollection
		{
			get { return (PeriodReopenLevelsCollection)GetParentCollection(this, typeof(PeriodReopenLevelsCollection)); }
		}

		#region Bound Properties

		#region Range

		[MaxLength(3)]
		[List("RangeList")]
		public ZString Range
		{
			get { return range; }
			set
			{
				CheckMaximumLength(RangeInfo, value);
				SetNonPersistentPropertyValue(RangeInfo, ref range, value);
				if (value == RangeCodes.Unlimited)
				{
					Days = 0;
				}
				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo RangeInfo
		{
			get { return GetZPropertyInfo(Schema.Range); }
		}

		public void ValidateRange()
		{
			ValidateRangeCore();
		}

		protected virtual void ValidateRangeCore()
		{
			RangeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RangeInfo);
			ListValidation.ErrorIfInvalidCode(RangeInfo, RangeList);
			if (ParentCollections.Count > 0 && Range == RangeCodes.Unlimited)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(RangeInfo, Res.GetString("9DFA2E44-140D-486c-A3C7-2A22067636C5", "There must be only one 'Unlimited' line."));
			}
		}

		ZString range;

		#endregion

		#region Days

		public virtual ZInt Days
		{
			get { return fDays; }
			set
			{
				SetNonPersistentPropertyValue(DaysInfo, ref fDays, value);
				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public virtual ZPropertyInfo DaysInfo
		{
			get { return GetZPropertyInfo(Schema.Days); }
		}

		protected bool Days_ReadOnly
		{
			get { return Range == RangeCodes.Unlimited; }
		}

		public void ValidateDays()
		{
			ValidateDaysCore();
		}

		protected virtual void ValidateDaysCore()
		{
			DaysInfo.ClearAllNotifications();
			if (Range != RangeCodes.Unlimited)
			{
				CompareValidation.CheckNumberGreaterThanZero(DaysInfo);
			}
			HigherDaysAmountHigherAuthorisationLevelValidation(DaysInfo);
		}

		protected void HigherDaysAmountHigherAuthorisationLevelValidation(ZPropertyInfo daysProperty)
		{
			if (!Range.IsEmpty && !AuthorisationRequirement.IsEmpty)
			{
				PeriodReopenLevelsCollection parentCollection = ParentCollection;
				if (parentCollection != null)
				{
					int currentWeight = GetAuthorisationRequirementWeight(AuthorisationRequirement);

					if (Range == RangeCodes.DaysAfterEndOfPeriod)
					{
						foreach (PeriodReopenLevels setting in parentCollection)
						{
							if (setting != this)
							{
								if (setting.Range == Range)
								{
									int settingWeight = GetAuthorisationRequirementWeight(setting.AuthorisationRequirement);
									ZInt settingAmount = (ZInt)setting[daysProperty.Name];
									if ((settingAmount <= (ZInt)daysProperty.Value && settingWeight > currentWeight) ||
										(settingAmount >= (ZInt)daysProperty.Value && settingWeight < currentWeight))
									{
										daysProperty.AddError(Res.GetString("1990B17E-96BB-464c-A9A0-570C667CA11B", "Higher days amounts must have Authorization level higher than lower days amounts."));
									}
								}
							}
						}
					}
					else if (Range == RangeCodes.Unlimited)
					{
						foreach (PeriodReopenLevels setting in parentCollection)
						{
							if (setting.Range == RangeCodes.DaysAfterEndOfPeriod && GetAuthorisationRequirementWeight(setting.AuthorisationRequirement) > currentWeight)
							{
								daysProperty.AddError(Res.GetString("5C56F1E9-B2C4-4f0b-8DE8-F3B007CA45A8", "The 'Unlimited' setting can only be used on the highest authorization level configured."));
								break;
							}
						}
					}
				}
			}
		}

		public int GetAuthorisationRequirementWeight(string authorisationRequirement)
		{
			return GetAuthorisationRequirementWeightCore(authorisationRequirement);
		}

		protected virtual int GetAuthorisationRequirementWeightCore(string authorisationRequirement)
		{
			int weight = -1;
			switch (authorisationRequirement)
			{
				case AuthorisationRequirementCodes.FirstApprovalRequired:
					weight = 0;
					break;
				case AuthorisationRequirementCodes.SecondApprovalRequired:
					weight = 1;
					break;
				case AuthorisationRequirementCodes.ThirdApprovalRequired:
					weight = 2;
					break;
			}
			return weight;
		}

		ZInt fDays;

		#endregion

		#region Authorisation Requirement

		[MaxLength(3)]
		[List("AuthorisationRequirementList")]
		public ZString AuthorisationRequirement
		{
			get { return authorisationRequirement; }
			set
			{
				CheckMaximumLength(AuthorisationRequirementInfo, value);
				SetNonPersistentPropertyValue(AuthorisationRequirementInfo, ref authorisationRequirement, value);
				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo AuthorisationRequirementInfo
		{
			get { return GetZPropertyInfo(Schema.AuthorisationRequirement); }
		}

		public void ValidateAuthorisationRequirement()
		{
			ValidateAuthorisationRequirementCore();
		}

		protected virtual void ValidateAuthorisationRequirementCore()
		{
			AuthorisationRequirementInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(AuthorisationRequirementInfo);
			ListValidation.ErrorIfInvalidCode(AuthorisationRequirementInfo, AuthorisationRequirementList);
			if (!AuthorisationRequirementInfo.HasErrors())
			{
				if (ParentCollections.Count > 0 && !AuthorisationRequirement.IsEmpty)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(AuthorisationRequirementInfo,
						string.Format(Res.GetString("226E1D84-9E43-4c04-8272-4E81D162AD2B", "There must be only one '{0}' line.", AuthorisationRequirement)));
				}
			}
		}

		ZString authorisationRequirement;

		#endregion

		#endregion

		#region Lookups

		#region Range List

		public static class RangeCodes
		{
			public const string DaysAfterEndOfPeriod = "DAE";
			public const string Unlimited = "UNL";
		}

		public CodeDescriptionPairList RangeList
		{
			get
			{
				if (fRangeList == null)
				{
					fRangeList = new CodeDescriptionPairList();
					fRangeList.AddPair(RangeCodes.DaysAfterEndOfPeriod, Res.GetString("FFC58925-4E39-425c-BEB6-D36FCDBFCF55", "Days After End Of Period"));
					fRangeList.AddPair(RangeCodes.Unlimited, Res.GetString("A1D2B653-68B8-49e1-9634-701131152263", "Unlimited"));
				}

				return fRangeList;
			}
		}

		CodeDescriptionPairList fRangeList;

		#endregion

		#region Authorisation Requirement List

		public static class AuthorisationRequirementCodes
		{
			public const string FirstApprovalRequired = "1ST";
			public const string SecondApprovalRequired = "2ND";
			public const string ThirdApprovalRequired = "3RD";
		}

		public CodeDescriptionPairList AuthorisationRequirementList
		{
			get
			{
				return fAuthorisationRequirementList ?? (fAuthorisationRequirementList = GetAuthorisationRequirementList());
			}
		}

		protected virtual CodeDescriptionPairList GetAuthorisationRequirementList()
		{
			CodeDescriptionPairList resultPairList = new CodeDescriptionPairList();
			resultPairList.AddPair(AuthorisationRequirementCodes.FirstApprovalRequired, Res.GetString("4ED9F44C-A326-4c7c-9630-D714803DA259", "1st Level"));
			resultPairList.AddPair(AuthorisationRequirementCodes.SecondApprovalRequired, Res.GetString("F2794758-B9C6-4461-93D5-686EFBA42C93", "2nd Level"));
			resultPairList.AddPair(AuthorisationRequirementCodes.ThirdApprovalRequired, Res.GetString("957F7878-C629-486e-BC49-E1D1778558AC", "3rd Level"));
			return resultPairList;
		}

		CodeDescriptionPairList fAuthorisationRequirementList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Days, Days.ToString());
			writer.WriteElementString(Schema.Range, Range);
			writer.WriteElementString(Schema.AuthorisationRequirement, AuthorisationRequirement);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Days = ZInt.Parse(reader.ReadElementString(Schema.Days));
			Range = reader.ReadElementString(Schema.Range);
			AuthorisationRequirement = reader.ReadElementString(Schema.AuthorisationRequirement);
		}

		#endregion
	}
}