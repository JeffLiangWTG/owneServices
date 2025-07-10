using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldCountryValidation : StmSystemDefinedFieldValidation
	{
		public StmSystemDefinedFieldCountryValidation(StmSystemDefinedFieldCountry parent) : base(parent)
		{
		}

		protected new StmSystemDefinedFieldCountry Parent
		{
			get { return (StmSystemDefinedFieldCountry)base.Parent; }
		}

		protected override void CheckS1_RN_NKCntrySpecific()
		{
			base.CheckS1_RN_NKCntrySpecific();
			MandatoryValidation.CheckEntered(Parent.S1_RN_NKCntrySpecificInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S1_RN_NKCntrySpecificInfo);

			if (!Parent.S1_RN_NKCntrySpecificInfo.HasErrors() && ((IBusinessObjectInternals)Parent).ParentCollections.Length > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.S1_RN_NKCntrySpecificInfo);
			}
		}

		#region S1_IsSuppressed

		protected override void CheckS1_IsSuppressed()
		{
			base.CheckS1_IsSuppressed();

			if (Parent.S1_IsSuppressed && ((IBusinessObjectInternals)Parent).ParentCollections.Length == 1 && ((IBusinessObjectInternals)Parent).ParentCollections[0].TypeOfElements == typeof(StmSystemDefinedFieldCountry))
			{
				StringCollectionX countryCodes = new StringCollectionX();

				foreach (StmSystemDefinedFieldCountry fieldCountry in ((IBusinessObjectInternals)Parent).ParentCollections[0])
				{
					if (fieldCountry != Parent && fieldCountry.CntrySpecific != null && !fieldCountry.S1_IsSuppressed)
					{
						countryCodes.Add(fieldCountry.CntrySpecific.RN_Code);
					}
				}

				if (countryCodes.Count > 0)
				{
					Parent.S1_IsSuppressedInfo.AddError(Res.GetString("5b30756c-a360-4e71-91d4-cdf1551f3678", "This field is already country/region specific for {0}. There is no need to specify a suppressed country/region.", GetFormattedCountryCodeList(countryCodes)));
				}
			}
		}

		string GetFormattedCountryCodeList(StringCollectionX countryCodes)
		{
			StringBuilder result = new StringBuilder();
			result.Append(countryCodes[0]);

			for (int i = 1; i < countryCodes.Count; i++)
			{
				if (i == countryCodes.Count - 1)
				{
					result.Append(" " + Res.GetString("d0c2faec-0df7-4082-9da6-12c1f9f713a5", "and") + " ");
					result.Append(countryCodes[i]);
				}
				else
				{
					result.Append(", ");
					result.Append(countryCodes[i]);
				}
			}

			return result.ToString();
		}

		#endregion
	}
}
