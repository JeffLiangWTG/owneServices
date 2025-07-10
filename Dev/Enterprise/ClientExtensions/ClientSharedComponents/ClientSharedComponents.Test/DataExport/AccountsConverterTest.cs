using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents.Testing
{
	public abstract class AccountsConverterTest : TestCaseWithFactory
	{
		public void TestBaseImplementation()
		{
			AccountsConverterForTest converter = new AccountsConverterForTest(Factory, new NotificationBuffer());
			Assert("Not run yet", !converter.ExportAccountsHasRun);
			converter.MapExport(new Xsd.TxnHeader());
			Assert("Has run", converter.ExportAccountsHasRun);
		}

		class AccountsConverterForTest : AccountsConverter
		{
			public AccountsConverterForTest(BusinessObjectFactory factory, NotificationBuffer notifications)
				: base(factory, notifications)
			{
				ExportAccountsHasRun = false;
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}

			protected override bool IsOkToProcess(Xsd.TxnHeader xmlHeader)
			{
				return true;
			}

			protected override FlatFileDataRowCollection ExportAccounts(Xsd.TxnHeader xmlHeader)
			{
				ExportAccountsHasRun = true;
				return new FlatFileDataRowCollection();
			}

			public bool ExportAccountsHasRun;

			protected override ZBool fCheckThatAllTransactionsAreExported
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}
