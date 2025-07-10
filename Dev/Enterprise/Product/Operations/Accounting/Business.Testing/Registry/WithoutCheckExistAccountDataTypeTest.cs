using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(WithoutCheckExistAccountDataType))]
	class WithoutCheckExistAccountDataTypeTest : GeneralLedgerRegistryDataTypeTest
	{
		public WithoutCheckExistAccountDataTypeTest()
		{
			HeaderLinesCauseValidationError = new List<HeaderLine>();

			var line = new Line() { PK = Guid.NewGuid(), AH = Guid.Empty, IsGST = false, LineType = "WIP", GSTVATBasis = "A", PostToGL = "Y", IsInsertAccCashBasisVAT = false, IsDeleteAccCashBasisVATQueue = false };
			var headerLine = new HeaderLine() { Header = null, Line = line };
			HeaderLinesNotCauseValidationError = new List<HeaderLine>();
			HeaderLinesNotCauseValidationError.Add(headerLine);
		}

		protected override Type TypeOfGeneralLedgerRegistryDataType
		{
			get { return typeof(WithoutCheckExistAccountDataType); }
		}
	}
}
