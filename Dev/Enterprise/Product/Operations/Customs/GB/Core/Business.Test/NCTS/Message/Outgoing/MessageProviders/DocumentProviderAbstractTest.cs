using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestsSubclassesOf(typeof(DocumentProvider))]
	abstract class DocumentProviderAbstractTest<T> : DataProviderTestCase<T> where T : DocumentProvider
	{
		protected override T GetProvider() => provider;

		protected abstract string SubType { get; }

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			info = Factory.CreateCusSupportingInfo("OTH", SubType, "RefNum", null, "TYP", header);

			provider = (T)Activator.CreateInstance(typeof(T), info, false);
		}

		T provider;
		protected CusSupportingInfo info;
	}
}
