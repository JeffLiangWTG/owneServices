using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed
		{
			get { return LocalCurrencyCodeCore; }
		}

		public bool IsThrowingAwayMergeReturns { get; set; }

		public Directions GetJobDirectionExposed()
		{
			return GetJobDirection();
		}

		public override bool IsThrowingAwayMerge => IsThrowingAwayMergeReturns;
	}
}
