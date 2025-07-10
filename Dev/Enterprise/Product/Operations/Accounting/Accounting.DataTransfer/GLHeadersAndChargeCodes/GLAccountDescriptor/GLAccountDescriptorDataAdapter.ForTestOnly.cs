#if DEBUG

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	public partial class GLAccountDescriptorDataAdapter
	{
		public void ImportFromValueObjectCore_ForTestOnly(AccGLAccountDescriptor glAccDescriptorBizObj, Enterprise.DataTransfer.Xml.XsdVersion1.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping value, IValueObjectImportContext context)
		{
			ImportFromValueObjectCore(glAccDescriptorBizObj, value, context);
		}

		public void NotifyBizObjCreatedOrUpdated_ForTestOnly(INotifications notifications, BusinessObject bizObj)
		{
			NotifyBizObjCreatedOrUpdated(notifications, bizObj);
		}
	}
}

#endif
