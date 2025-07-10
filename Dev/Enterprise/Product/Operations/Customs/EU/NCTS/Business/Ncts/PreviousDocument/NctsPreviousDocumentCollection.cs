using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsPreviousDocumentCollection<out TNCTSPreviousDocument> :
		ICusSupportingInfoCollection<TNCTSPreviousDocument>, ISequenceNumberHeader
		where TNCTSPreviousDocument : NctsPreviousDocument
	{
		new TNCTSPreviousDocument this[int index] { get; }
		new TNCTSPreviousDocument AddNew();
		ShortSequenceNumberGenerator SequenceGenerator { get; }
		void MaxCountValidationEnable(int maxCount);
	}

	public class NctsPreviousDocumentCollection<TNCTSPreviousDocument> :
		CusSupportingInfoCollection<TNCTSPreviousDocument>, INctsPreviousDocumentCollection<TNCTSPreviousDocument>
		where TNCTSPreviousDocument : NctsPreviousDocument
	{
		public NctsPreviousDocumentCollection(
			BusinessObject parent,
			Action<NctsPreviousDocument> setDefaultsForNewChild = null,
			Func<ZQuery> additionalFilter = null)
		: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
			this.additionalFilter = additionalFilter;
			this.setDefaultsForNewChild = setDefaultsForNewChild;

			EnableMaxCountValidationIfRequired();
		}

		readonly Func<ZQuery> additionalFilter;
		readonly Action<NctsPreviousDocument> setDefaultsForNewChild;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this;

		public ShortSequenceNumberGenerator SequenceGenerator => fSequenceGenerator ?? (fSequenceGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator fSequenceGenerator;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			setDefaultsForNewChild?.Invoke((NctsPreviousDocument)child);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return additionalFilter == null ? base.CreateAdditionalFilter() : base.CreateAdditionalFilter().AddToFilter(additionalFilter());
		}

		void EnableMaxCountValidationIfRequired()
		{
			if (Master is NctsDepartureCargoDesc master
				&& master.Header is NctsHeader nctsHeader
				&& nctsHeader.IsPhase5)
			{
				var validationRuleConfiguration = nctsHeader.Configuration.ValidationRuleConfiguration;
				var isInTransitionPeriod = nctsHeader.IsInPhase5TransitionPeriod;
				const int MaxCountForRuleTR0030 = 99;
				const int MaxCountForRuleE1401_1 = 9;

				if (isInTransitionPeriod && validationRuleConfiguration.IsRuleE1401_1Active)
				{
					MaxCountValidationWithMessageErrorEnable(MaxCountForRuleE1401_1, Res.GetString("F890AA20-0F52-4C5D-90F7-CC957013853A", "{0} In transition period, which is now, a maximum of {1} lines can be entered.", validationRuleConfiguration.Messages.E1401_1RuleCode.GetRuleCodeMessagePrefix(), MaxCountForRuleE1401_1));
				}
				else if (!isInTransitionPeriod && validationRuleConfiguration.IsRuleTR0030Active)
				{
					MaxCountValidationWithMessageErrorEnable(MaxCountForRuleTR0030, Res.GetString("B671E134-6830-40A0-92E3-FA877671D4DA", "{0} The maximum number of {1} Previous Documents has been exceeded.", ValidationRuleCodeConstants.TR0030.GetRuleCodeMessagePrefix(), MaxCountForRuleTR0030));
				}
			}
		}
	}
}
