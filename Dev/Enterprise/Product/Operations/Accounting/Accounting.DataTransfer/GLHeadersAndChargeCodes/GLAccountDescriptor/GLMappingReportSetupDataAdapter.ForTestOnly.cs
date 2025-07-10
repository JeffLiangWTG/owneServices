#if DEBUG

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	public partial class GLMappingReportSetupDataAdapter
	{
		public void ImportFromValueObjectCore_ForTestOnly(GLDescriptorPivot newGLDescriptorPivot, Enterprise.DataTransfer.Xml.XsdVersion1.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(newGLDescriptorPivot, value, context);
		}

		public void NotifyBizObjCreatedOrUpdated_ForTestOnly(INotifications notifications, BusinessObject bizObj)
		{
			NotifyBizObjCreatedOrUpdated(notifications, bizObj);
		}
	}
}

#endif
