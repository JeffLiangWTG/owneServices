using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(AccruedRevenueControlAccountDataType))]
	class AccruedRevenueControlAccountDataTypeTest : GeneralLedgerRegistryDataTypeTest
	{
		#region Implementation

		public AccruedRevenueControlAccountDataTypeTest()
		{
			HeaderLinesCauseValidationError = new List<HeaderLine>();
			var line = new Line() { PK = Guid.NewGuid(), AH = Guid.Empty, IsGST = false, LineType = "WIP", GSTVATBasis = "A", PostToGL = "Y", IsInsertAccCashBasisVAT = false, IsDeleteAccCashBasisVATQueue = false };
			var headerLine = new HeaderLine() { Header = null, Line = line };
			HeaderLinesCauseValidationError.Add(headerLine);

			line = new Line() { PK = Guid.NewGuid(), AH = Guid.Empty, IsGST = false, LineType = "ACR", GSTVATBasis = "A", PostToGL = "Y", IsInsertAccCashBasisVAT = false, IsDeleteAccCashBasisVATQueue = false };
			headerLine = new HeaderLine() { Header = null, Line = line };
			HeaderLinesNotCauseValidationError = new List<HeaderLine>();
			HeaderLinesNotCauseValidationError.Add(headerLine);
		}

		protected override Type TypeOfGeneralLedgerRegistryDataType
		{
			get { return typeof(AccruedRevenueControlAccountDataType); }
		}

		#endregion
	}
}
