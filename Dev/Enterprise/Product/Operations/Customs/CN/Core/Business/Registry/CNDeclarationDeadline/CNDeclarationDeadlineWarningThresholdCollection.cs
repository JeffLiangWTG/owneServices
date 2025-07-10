using System.Drawing;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.CN.Business.XmlSerializers")]
	public class CNDeclarationDeadlineWarningThresholdCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CNDeclarationDeadlineWarningThresholdCollection()
			: base(new BusinessObjectFactory { NameForDebugging = "CNDocTemplateForAttachmentCollection_Ctor" })
		{
		}

		public CNDeclarationDeadlineWarningThresholdCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
		{
		}

		public new CNDeclarationDeadlineWarningThreshold this[int i] => (CNDeclarationDeadlineWarningThreshold)Elements[i];

		public new CNDeclarationDeadlineWarningThreshold AddNew() => (CNDeclarationDeadlineWarningThreshold)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject()
				=> new CNDeclarationDeadlineWarningThreshold(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				=> new CNDeclarationDeadlineWarningThresholdCollection(fallbackLevel, factory);

		public static CNDeclarationDeadlineWarningThresholdCollection GetDefault()
		{
			var result = new CNDeclarationDeadlineWarningThresholdCollection();

			SetupDefaultFields(result.AddNew(), CNDeclarationDeadlineWarningThreshold.ALL, 1, Color.Red, 3, Color.LightSalmon, 7, Color.LightYellow);

			return result;
		}
		static void SetupDefaultFields(CNDeclarationDeadlineWarningThreshold defaultDeadlineWarning, ZString transportMode,
			ZInt firstThreshold, Color firstColor,
			ZInt secondThreshold, Color secondColor,
			ZInt thirdThreshold, Color thirdColor)
		{
			using (defaultDeadlineWarning.GetValidationSuspender())
			{
				defaultDeadlineWarning.TransportMode = transportMode;
				defaultDeadlineWarning.FirstLevelThreshold = firstThreshold;
				defaultDeadlineWarning.FirstLevelWarningColor = ColorHelper.GetRGBbyColor(firstColor);
				defaultDeadlineWarning.SecondLevelThreshold = secondThreshold;
				defaultDeadlineWarning.SecondLevelWarningColor = ColorHelper.GetRGBbyColor(secondColor);
				defaultDeadlineWarning.ThirdLevelThreshold = thirdThreshold;
				defaultDeadlineWarning.ThirdLevelWarningColor = ColorHelper.GetRGBbyColor(thirdColor);
			}
		}
	}
}
