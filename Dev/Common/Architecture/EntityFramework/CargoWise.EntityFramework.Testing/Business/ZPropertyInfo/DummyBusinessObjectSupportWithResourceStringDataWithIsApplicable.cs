using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework.Testing
{
	public sealed class DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable : DummyBusinessObject, ISupportMultipleResourceStringData
	{
		public DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("21a95a6b-2614-45e2-b02d-6de4a493f0b7", Caption = "Description_1", MultipleKey = "DummyMultipleKey", IsApplicableMember = nameof(IsDummyValueEqualToOne))]
		[ResourceStringData("DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable|No_Multiple_Key|IsDummyValueEqualToOne|Z0_Description", Caption = "Description_2", IsApplicableMember = nameof(IsDummyValueEqualToOne))]
		[ResourceStringData("771c4aa8-5c36-4d9b-aa1c-543f3afc3b6c", Caption = "Description_3")]
		[ResourceStringData("23944c91-a66d-488f-abdf-c6f177cba4df", Caption = "Description_4", MultipleKey = "DummyMultipleKeyAnother")]
		public override ZString Z0_Description { get; set; }

		[ResourceStringData("c8312279-63c1-472b-978e-778a03e35f9b", Caption = "Number", MultipleKey = "KEY_123")]
		[ResourceStringData("4eb1e75b-3b95-4ba1-b929-de72ebcd56b5", Caption = "Number_Description_True", IsApplicableMember = nameof(IsDummyValueEqualToOne))]
		public override ZInt Z0_Number { get; set; }

		[ResourceStringData("a50fbe1d-f490-4cbb-b619-318bdc8994b3", Caption = "Date_None")]
		[ResourceStringData("7ec6679a-d7bb-4f15-8f57-e6c136c91b92", Caption = "Date_One", MultipleKey = "DummyMultipleKey", IsApplicableMember = nameof(IsDummyValueEqualToOne))]
		[ResourceStringData("e70e9754-02f5-4afb-80a8-7cedd2e7e998", Caption = "Date_Two", MultipleKey = "DummyMultipleKey", IsApplicableMember = nameof(IsDummyValueEqualToTwo))]
		public override ZDateTime Z0_AnotherDate { get; set; }

		public bool IsDummyValueEqualToOne => dummyField == 1;

		public bool IsDummyValueEqualToTwo => dummyField == 2;

		public void SetDummyValue(int value) => dummyField = value;

		public IReadOnlyList<string> MultipleKeysToUse => new[] { "DummyMultipleKey", "DummyMultipleKeyAnother" };

		int dummyField;
	}
}
