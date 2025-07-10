using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface ICommonPreviousDocumentCollection<out TCommonPreviousDocument> :
		ICusSupportingInfoCollection<TCommonPreviousDocument>, ISequenceNumberHeader
		where TCommonPreviousDocument : CommonPreviousDocument
	{
		new TCommonPreviousDocument this[int index] { get; }
		new TCommonPreviousDocument AddNew();
		ShortSequenceNumberGenerator SequenceGenerator { get; }
	}

	public class CommonPreviousDocumentCollection<TCommonPreviousDocument>
		: CusSupportingInfoCollection<TCommonPreviousDocument>, ICommonPreviousDocumentCollection<TCommonPreviousDocument>
		where TCommonPreviousDocument : CommonPreviousDocument
	{
		public CommonPreviousDocumentCollection(NctsBill parent) : this((BusinessObject)parent)
		{
			SetEnableMaxCountValidationForBillParent(parent);
		}

		void SetEnableMaxCountValidationForBillParent(NctsBill bill)
		{
			var header = bill.Header;
			if (header is null)
			{
				return;
			}

			if (bill.ValidationDecider is INctsBillDeparturePhase5ValidationDecider { IsRuleG0026_1Active: true })
			{
				EnableMaxCountValidation(MaxCountForBillForG0026_1, ValidationRuleCodeConstants.G0026_1);
			}
			else if (header.Configuration.ValidationRuleConfiguration.IsRuleTR0030Active)
			{
				EnableMaxCountValidation(MaxCountForBill, ValidationRuleCodeConstants.TR0030);
			}
		}

		public CommonPreviousDocumentCollection(NctsHeader parent) : this((BusinessObject)parent)
		{
			if (parent.Configuration.ValidationRuleConfiguration.IsRuleTR0028Active)
			{
				EnableMaxCountValidation(MaxCountForHeader, ValidationRuleCodeConstants.TR0028);
			}
		}

		CommonPreviousDocumentCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
		}

		protected virtual void EnableMaxCountValidation(int maxCount, string rule)
		{
			MaxCountValidationWithMessageErrorEnable(maxCount, $"[{rule}] The maximum number of {maxCount} Previous Documents has been exceeded.");
		}

		public override void RemoveAndDeleteAll()
		{
			using (SequenceGenerator.GetLineNumberSuspender())
			{
				base.RemoveAndDeleteAll();
			}
		}

		protected virtual int MaxCountForBill => 99;

		protected virtual int MaxCountForHeader => 9999;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this;

		ShortSequenceNumberGenerator sequenceGenerator;
		public ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(this));

		const int MaxCountForBillForG0026_1 = 1;
	}
}
