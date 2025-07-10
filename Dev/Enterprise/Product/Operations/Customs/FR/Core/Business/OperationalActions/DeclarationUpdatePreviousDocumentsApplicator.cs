using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class DeclarationUpdatePreviousDocumentsApplicator : EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator
	{
		public DeclarationUpdatePreviousDocumentsApplicator(BusinessObjectFactory factory, ZString dataGroupingCode) : base(factory, dataGroupingCode)
		{
		}

		public override CodeDescriptionPairList DocumentCodeList => Factory.GetCachedValue<PreviousDocumentCodeList>();
	}
}
