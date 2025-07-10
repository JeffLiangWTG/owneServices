using System;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(DocumentProvider))]
	abstract class DocumentProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : DocumentProvider
	{
		protected override T GetProvider() => provider;

		protected abstract string SubType { get; }

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			info = Factory.CreateCusSupportingInfo("OTH", SubType, "RefNum", null, "TYP", header);

			provider = (T)Activator.CreateInstance(typeof(T), info);
		}

		T provider;
		protected CusSupportingInfo info;
	}
}
