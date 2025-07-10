using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsSupportingDocumentCollection<out T> : ISequenceNumberHeader, EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentCollection<T>
		where T : NctsSupportingDocument
	{
		ShortSequenceNumberGenerator SequenceGenerator { get; }

		new T this[int index] { get; }

		new T AddNew();
	}

	public class NctsSupportingDocumentCollection<T> : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection<T>, INctsSupportingDocumentCollection<T>
		where T : NctsSupportingDocument
	{
		public NctsSupportingDocumentCollection(BusinessObject parent)
			: base(parent)
		{
			EnableMaxcountValidation();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Master is ICanSupportPhase5 masterSupportsPhase5 && masterSupportsPhase5.IsPhase5Arrival)
			{
				var supportingDocument = child as NctsSupportingDocument;
				supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
			}
		}

		public override void RemoveAndDeleteAll()
		{
			using (SequenceGenerator.GetLineNumberSuspender())
			{
				base.RemoveAndDeleteAll();
			}
		}

		public IEnumerable<ISequenceNumberLine> Lines => this;

		public ShortSequenceNumberGenerator SequenceGenerator => sequenceNumberGenerator ?? (sequenceNumberGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator sequenceNumberGenerator;

		void EnableMaxcountValidation()
		{
			if (Master is ICanSupportPhase5 master && master.IsPhase5Departure)
			{
				if (IsPhase5DepartureMaxCountValidationEnabled)
				{
					var maxSupportingDocumentsAllowed = 99;
					MaxCountValidationWithMessageErrorEnable(maxSupportingDocumentsAllowed, Res.GetString("9B5490F2-98AD-412D-9A01-578C38CEF5B0", "[{0}] The maximum number of {1} Supporting Documents has been exceeded.", ValidationRuleCodeConstants.TR0029, maxSupportingDocumentsAllowed));
				}
			}
			else
			{
				MaxCountValidationEnable(99);
			}
		}

		bool IsPhase5DepartureMaxCountValidationEnabled
		{
			get
			{
				NctsHeader header = null;
				switch (Master)
				{
					case NctsCommonCargoDesc goodsItem:
						header = goodsItem.Header;
						break;
					case NctsBill bill:
						header = bill.Header;
						break;
					case NctsDepartureMovementHeader moveHeader:
						header = moveHeader.Header;
						break;
				}

				return header?.Configuration.ValidationRuleConfiguration.IsRuleTR0029Active ?? false;
			}
		}
	}
}
