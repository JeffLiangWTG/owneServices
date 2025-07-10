using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBusinessObjectSupportMultipleResourceStringData1 : DummyBusinessObject, ISupportMultipleResourceStringData
	{
		public DummyBusinessObjectSupportMultipleResourceStringData1(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("ECC|DummyBusinessObject|Z0_Description", Caption = "[5/34] Description", MultipleKey = "DummyBusinessObject|ECC")]
		[ResourceStringData("SAD|DummyBusinessObject|Z0_Description", Caption = "[21] Description", MultipleKey = "DummyBusinessObject|SAD")]
		[ResourceStringData("BLANK|DummyBusinessObject|Z0_Description", Caption = "[54] Test")]
		public override ZString Z0_Description { get; set; }

		[ResourceStringData("DummyBusinessObject|Z0_Number", Caption = "Number")]
		public override ZInt Z0_Number { get; set; }

		public virtual bool IsECCCompliant { get; set; }

		public IReadOnlyList<string> MultipleKeysToUse => MultipleKeysToUseForTesting ?? (IsECCCompliant ? new[] { "DummyBusinessObject|ECC" } : new[] { "DummyBusinessObject|SAD" });
		public IReadOnlyList<string> MultipleKeysToUseForTesting;
	}
}
