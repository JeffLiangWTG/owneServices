using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.DataItemIDAttribute;

namespace Enterprise.Customs.KR.Business
{
	public class AmendedItem : NonPersistentBusinessObject
	{
		public ZString EntityType { get; set; }
		public EntityAmendType AmendType { get; set; }
		public ChangeType ChangeType { get; set; }
		public IEnumerable<IDInList> IDsInList { get; set; }

		[ResourceStringData("AmendedItem|DataItemID", Caption = "Data Item ID")]
		public ZString DataItemID { get; set; }

		[ResourceStringData("AmendedItem|DataItemDescription", Caption = "Amended Field")]
		public ZString DataItemDescription { get; set; }

		[ResourceStringData("AmendedItem|BeforeValue", Caption = "Previous Value")]
		public ZString BeforeValue { get; set; }

		[ResourceStringData("AmendedItem|AfterValue", Caption = "Amended Value")]
		public ZString AfterValue { get; set; }

		[ResourceStringData("AmendedItem|AmendTypeDescription", Caption = "Type")]
		public ZString AmendTypeDescription => AmendType.ToString();

		[ResourceStringData("AmendedItem|ID", Caption = "Changed Item")]
		public ZString ID
		{
			get
			{
				if (!id.HasValue)
				{
					id = IDsInList.GetID();
				}
				return id.Value;
			}
		}
		ZString? id;

		[ResourceStringData("AmendedItem|DutyTaxType", Caption = "Duty Tax Type")]
		public ZString DutyTaxType { get; set; }

		[ResourceStringData("AmendedItem|DutyTaxTypeDescription", Caption = "Duty Tax Type Desc.")]
		public ZString DutyTaxTypeDescription { get; set; }

		[ResourceStringData("AmendedItem|BeforeAmount", Caption = "Previous Amount")]
		public ZDecimal BeforeAmount => IsDutyTaxItemField ? ZDecimal.ParseSafe(BeforeValue, 0) : ZDecimal.Zero;

		[ResourceStringData("AmendedItem|AfterAmount", Caption = "Amended Amount")]
		public ZDecimal AfterAmount => IsDutyTaxItemField ? ZDecimal.ParseSafe(AfterValue, 0) : ZDecimal.Zero;

		[ResourceStringData("AmendedItem|AmountDifference", Caption = "Duty Tax Difference")]
		public ZDecimal AmountDifference => IsDutyTaxItemField ? AfterAmount - BeforeAmount : ZDecimal.Zero;

		ZBool IsDutyTaxItemField => ImportAmendmentDataItemIDList.IsDutyTaxItemField(DataItemID);
	}
}
