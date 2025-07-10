using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class DocPackage : DocumentWrapper
	{
		DocPackage(BasePackage package, BusinessObjectFactory factoryToWrap)
			: base(package, factoryToWrap)
		{
			languageCode = TranslationHelper.GetLanguageCodeForCountry(Core.Constants.CountryCodes.Brazil);
		}

		readonly ZString languageCode;

		public static DocPackage New(BasePackage package, BusinessObjectFactory factoryToWrap)
		{
			if (package == null)
			{
				return null;
			}
			else
			{
				return new DocPackage(package, factoryToWrap);
			}
		}

		#region PackQTYWrapper

		public PackQTYWrapper Pack => pack ?? (pack = new PackQTYWrapper(BasePackage.CW_PackQty, BasePackage.CW_PackType, PackingTypesList, Factory));
		PackQTYWrapper pack;

		#endregion

		#region ZString

		public ZString Unit => Pack.Unit.Code;

		#endregion

		#region Implementation

		BasePackage BasePackage
		{
			get { return (BasePackage)WrappedObject; }
		}

		CodeDescriptionPairList PackingTypesList => BRRefCusCodeListTypes.GetPackagesTypesList(Factory, languageCode);

		#endregion
	}
}
