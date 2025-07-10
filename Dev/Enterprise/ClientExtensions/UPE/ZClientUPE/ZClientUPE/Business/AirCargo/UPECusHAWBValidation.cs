using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.CMR;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBValidation : Customs.Business.AutoCusHAWBValidation
	{
		public UPECusHAWBValidation(UPECusHAWB parent)
			: base(parent)
		{
		}

		public void ValidateBillingTerms()
		{
			ValidateCalculatedProperty(Parent.BillingTermsInfo);
		}

		protected virtual void CheckBillingTerms()
		{
			MandatoryValidation.CheckEntered(Parent.BillingTermsInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BillingTermsInfo, Parent.Lookups.PrepaidCollectList);
		}

		#region Profiling and Screening

		protected override void CheckCS_GoodsDescription()
		{
			base.CheckCS_GoodsDescription();
			ZString goodsDescriptionStopPhraseWarning = Screening.GoodsDescriptionStopPhraseWarning;
			if (!goodsDescriptionStopPhraseWarning.IsEmpty)
			{
				Parent.CS_GoodsDescriptionInfo.AddWarning(goodsDescriptionStopPhraseWarning);
			}
		}

		protected override void CheckCS_ConsignorName()
		{
			base.CheckCS_ConsignorName();
			ValidateStopPhrase(Parent.CS_ConsignorNameInfo, Screening.UPSStopPhrasesFoundInConsignorName);
		}

		protected override void CheckCS_ConsignorStreet()
		{
			base.CheckCS_ConsignorStreet();
			ValidateStopPhrase(Parent.CS_ConsignorStreetInfo, Screening.UPSStopPhrasesFoundInConsignorStreet);
		}

		protected override void CheckCS_ConsignorStreet2()
		{
			base.CheckCS_ConsignorStreet2();
			ValidateStopPhrase(Parent.CS_ConsignorStreet2Info, Screening.UPSStopPhrasesFoundInConsignorStreet2);
		}

		public void ValidateLevel1RecordConsignorAccountNum()
		{
			ValidateCalculatedProperty(Parent.Level1RecordConsignorAccountNumInfo);
		}

		protected void CheckLevel1RecordConsignorAccountNum()
		{
			ValidateStopPhrase(Parent.Level1RecordConsignorAccountNumInfo, Screening.UPSStopPhrasesFoundInConsignorAccountNum);
		}

		protected override void CheckCS_ConsigneeName()
		{
			base.CheckCS_ConsigneeName();
			ValidateStopPhrase(Parent.CS_ConsigneeNameInfo, Screening.UPSStopPhrasesFoundInConsigneeName);
		}

		protected override void CheckCS_ConsigneeStreet()
		{
			base.CheckCS_ConsigneeStreet();
			ValidateStopPhrase(Parent.CS_ConsigneeStreetInfo, Screening.UPSStopPhrasesFoundInConsigneeStreet);
		}

		protected override void CheckCS_ConsigneeStreet2()
		{
			base.CheckCS_ConsigneeStreet2();
			ValidateStopPhrase(Parent.CS_ConsigneeStreet2Info, Screening.UPSStopPhrasesFoundInConsigneeStreet2);
		}

		public void ValidateLevel1RecordConsigneeAccountNum()
		{
			ValidateCalculatedProperty(Parent.Level1RecordConsigneeAccountNumInfo);
		}

		protected void CheckLevel1RecordConsigneeAccountNum()
		{
			ValidateStopPhrase(Parent.Level1RecordConsigneeAccountNumInfo, Screening.UPSStopPhrasesFoundInConsigneeAccountNum);
		}

		protected override void CheckCS_GoodsValue()
		{
			base.CheckCS_GoodsValue();
			if (Screening.IsGoodsValueIdentified)
			{
				Parent.CS_GoodsValueInfo.AddWarning("Goods value identified for UPS screening");
			}
			ValidateCS_GoodsDescription();
		}

		void ValidateStopPhrase(ZPropertyInfo property, IEnumerable<ZString> stopPhrases)
		{
			ValidateStopPhrase(property, string.Join(", ", stopPhrases));
		}

		void ValidateStopPhrase(ZPropertyInfo property, ZString stopPhraseWarning)
		{
			if (!stopPhraseWarning.IsEmpty)
			{
				property.AddWarning("UPS Stop Words found: " + stopPhraseWarning);
			}
		}

		UPEScreening Screening
		{
			get { return new UPEScreening(Parent); }
		}

		#endregion

		#region Implementation

		protected new UPECusHAWB Parent
		{
			get { return (UPECusHAWB)base.Parent; }
		}

		#endregion
	}
}
