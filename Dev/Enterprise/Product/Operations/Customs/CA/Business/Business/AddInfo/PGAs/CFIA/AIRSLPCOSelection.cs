using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class AIRSLPCOSelection : AutoAIRSLPCOSelection
	{
		public AIRSLPCOSelection(BusinessObjectFactory factory, bool isChildGroup = false)
			: base(factory)
		{
			this.isChildGroup = isChildGroup;
		}
		readonly ZBool isChildGroup;

		ZString lPCOAndRegistrationForShow;

		public override ZString AL_LPCOAndRegistrationForShow => lPCOAndRegistrationForShow;

		public CodeDescriptionPairList MaterializedLPCOs => materializedLPCOs ?? (materializedLPCOs = new CodeDescriptionPairList());
		CodeDescriptionPairList materializedLPCOs;

		public CodeDescriptionPairList DeMaterializedLPCOs => deMaterializedLPCOs ?? (deMaterializedLPCOs = new CodeDescriptionPairList());
		CodeDescriptionPairList deMaterializedLPCOs;

		public CodeDescriptionPairList AIRSRegistrations => aIRSRegistrations ?? (aIRSRegistrations = new CodeDescriptionPairList());
		CodeDescriptionPairList aIRSRegistrations;

		public void BuildWebContent()
		{
			this.lPCOAndRegistrationForShow = AIRSHtmlHelper.BuildHtmlFromDataSet(Factory, MaterializedLPCOs, DeMaterializedLPCOs, AIRSRegistrations, this.isChildGroup);
		}
	}
}
