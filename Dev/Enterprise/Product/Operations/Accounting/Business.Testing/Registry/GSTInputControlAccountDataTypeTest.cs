using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GSTInputControlAccountDataType))]
	class GSTInputControlAccountDataTypeTest : GeneralLedgerRegistryDataTypeTest
	{
		#region Implementation

		public GSTInputControlAccountDataTypeTest()
		{
			HeaderLinesCauseValidationError = new List<HeaderLine>();
			var header = new Header() { PK = Guid.NewGuid(), Ledger = "AP", TransactionType = "INV", InvoiceNumber = "1" };
			var line = new Line() { PK = Guid.NewGuid(), AH = header.PK, IsGST = true, LineType = "CST", GSTVATBasis = "A", PostToGL = "N", IsInsertAccCashBasisVAT = false, IsDeleteAccCashBasisVATQueue = false };
			var headerLine = new HeaderLine() { Header = header, Line = line };
			HeaderLinesCauseValidationError.Add(headerLine);
			header = new Header() { PK = Guid.NewGuid(), Ledger = "AP", TransactionType = "INV", InvoiceNumber = "2" };
			line = new Line() { PK = Guid.NewGuid(), AH = header.PK, IsGST = true, LineType = "CST", GSTVATBasis = "C", PostToGL = "N", IsInsertAccCashBasisVAT = true, IsDeleteAccCashBasisVATQueue = true };
			headerLine = new HeaderLine() { Header = header, Line = line };
			HeaderLinesCauseValidationError.Add(headerLine);
			header = new Header() { PK = Guid.NewGuid(), Ledger = "CB", TransactionType = "DPY", InvoiceNumber = "3" };
			line = new Line() { PK = Guid.NewGuid(), AH = header.PK, IsGST = true, LineType = "CST", GSTVATBasis = "A", PostToGL = "N", IsInsertAccCashBasisVAT = true, IsDeleteAccCashBasisVATQueue = true };
			headerLine = new HeaderLine() { Header = header, Line = line };
			HeaderLinesCauseValidationError.Add(headerLine);

			header = new Header() { PK = Guid.NewGuid(), Ledger = "AR", TransactionType = "INV", InvoiceNumber = "4" };
			line = new Line() { PK = Guid.NewGuid(), AH = header.PK, IsGST = true, LineType = "REV", GSTVATBasis = "A", PostToGL = "N", IsInsertAccCashBasisVAT = true, IsDeleteAccCashBasisVATQueue = true };
			headerLine = new HeaderLine() { Header = header, Line = line };
			HeaderLinesNotCauseValidationError = new List<HeaderLine>();
			HeaderLinesNotCauseValidationError.Add(headerLine);
		}

		protected override Type TypeOfGeneralLedgerRegistryDataType
		{
			get { return typeof(GSTInputControlAccountDataType); }
		}

		#endregion
	}
}
