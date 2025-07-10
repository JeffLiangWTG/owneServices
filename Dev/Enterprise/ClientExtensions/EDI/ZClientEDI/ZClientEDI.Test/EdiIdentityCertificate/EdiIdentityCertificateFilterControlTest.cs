using System.Collections;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityCertificate.Module.Testing
{
	[TestedType(typeof(EdiIdentityCertificateFilterControl))]
	class EdiIdentityCertificateFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var collection = new EdiIdentityCertificateCollection(Factory);
			using (var filterControl = new EdiIdentityCertificateFilterControl(collection, new EdiIdentityCertificateFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				AssertHasColumn(columns, "ICE_CertificateThumbprint");
				AssertHasColumn(columns, "ICE_CertificateValidDate");
				AssertHasColumn(columns, "ICE_CertificateExpiryDate");
				AssertHasColumn(columns, "ICE_CertificateIssuedTo");
				AssertHasColumn(columns, "ICE_CertificateIssuedBy");
				AssertHasColumn(columns, "ICE_ProcessingStatus");
				AssertHasColumn(columns, "ICE_CARoot");
				AssertHasColumn(columns, "Application+IDA_ApplicationName");
				AssertHasColumn(columns, "Application+IDA_ClientID");
				AssertHasColumn(columns, "Application+IDA_IsRollback");
				AssertHasColumn(columns, "ICE_IsActive");
				AssertHasColumn(columns, "ICE_IsCertificateRevoked");
				AssertHasColumn(columns, "LicenseDatabase+LD_IsActive");
				AssertHasColumn(columns, "LicenseDatabase+EnterpriseID");
				AssertHasColumn(columns, "LicenseDatabase+EnterpriseCode");
				AssertHasColumn(columns, "LicenseDatabase+CompanyName");
				AssertHasColumn(columns, "LicenseDatabase+LD_Product");
				AssertHasColumn(columns, "LicenseDatabase+LD_LicenceType");
				AssertHasColumn(columns, "LicenseDatabase+CompanyCode");
				AssertHasColumn(columns, "LicenseDatabase+LD_ServerCode");
				AssertHasColumn(columns, "ICE_SequenceNumber");
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}
	}
}
