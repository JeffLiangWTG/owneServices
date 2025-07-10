using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.GUI.DevTools
{
	internal sealed class FieldBlobXmlTool : BlobXmlTool
	{
		public override string Name
		{
			get { return (NoResString)"Field Blob Xml"; }
		}

		protected override byte[] GetBlob(OperationalAction action)
		{
			return action.SU_ActionDataUpdateBlob;
		}
	}
}
