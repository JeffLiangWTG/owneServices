using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusHAWBSendersReferenceTest : SendersMessageReferenceProviderTest
	{
		protected override ISendersMessageReferenceProvider GetSavableProvider() => Factory.New<CTOCusMAWB>().ChildBills.AddNew();

		protected override ISendersMessageReferenceProvider GetProviderThatThrowsExceptionWhilstSaving() => Factory.New<CTOCusHAWBThatThrowsExceptionSaving>();

		sealed class CTOCusHAWBThatThrowsExceptionSaving : CTOCusHAWB
		{
			public CTOCusHAWBThatThrowsExceptionSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new ApplicationException("Test Helper Exception");
			}
		}
	}
}
