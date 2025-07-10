using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ARControlAccountDataType))]
	class ARControlAccountDataTypeTest : GeneralLedgerRegistryDataTypeTest
	{
		#region Implementation

		public ARControlAccountDataTypeTest()
		{
			var header = new Header() { PK = Guid.NewGuid(), Ledger = "AR", TransactionType = "REC", InvoiceNumber = "1" };
			var headerLine = new HeaderLine() { Header = header, Line = null };
			HeaderLinesCauseValidationError = new List<HeaderLine>();
			HeaderLinesCauseValidationError.Add(headerLine);

			header = new Header() { PK = Guid.NewGuid(), Ledger = "AP", TransactionType = "PAY", InvoiceNumber = "2" };
			headerLine = new HeaderLine() { Header = header, Line = null };
			HeaderLinesNotCauseValidationError = new List<HeaderLine>();
			HeaderLinesNotCauseValidationError.Add(headerLine);
		}

		protected override Type TypeOfGeneralLedgerRegistryDataType
		{
			get { return typeof(ARControlAccountDataType); }
		}

		protected override bool CannotContainSPRDissection => true;

		#endregion
	}
}
