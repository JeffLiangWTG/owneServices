using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.GUI.DevTools
{
	internal sealed class MethodBlobXmlTool : BlobXmlTool
	{
		public override string Name
		{
			get { return (NoResString)"Method Blob Xml"; }
		}

		protected override byte[] GetBlob(OperationalAction action)
		{
			return action.SU_ActionMenusAndMethodsBlob;
		}
	}
}
