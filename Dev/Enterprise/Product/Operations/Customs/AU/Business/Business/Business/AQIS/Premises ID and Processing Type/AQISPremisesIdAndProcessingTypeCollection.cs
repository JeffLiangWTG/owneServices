using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPremisesIdAndProcessingTypeCollection : AQISCollection<AQISPremisesIdAndProcessingType>
	{
		public AQISPremisesIdAndProcessingTypeCollection(BusinessObjectFactory factory, AUAddInfo addInfo)
			: base(factory, addInfo)
		{
		}

		protected override BusinessObject BusinessObjectToAddToCollection(ZString value1, ZString value2)
		{
			AQISPremisesIdAndProcessingType premisesIdAndProcessingType = new AQISPremisesIdAndProcessingType(Factory, AddInfo.JobDeclaration);
			premisesIdAndProcessingType.PremisesId = value1;
			premisesIdAndProcessingType.ProcessingType = value2;

			return premisesIdAndProcessingType;
		}

		public override void ReBuildAndSaveAQISElements()
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (AQISPremisesIdAndProcessingType premisesIdAndProcessingType in this)
			{
				result.Append(premisesIdAndProcessingType.PremisesId.ToString());
				result.Append("/");
				result.Append(premisesIdAndProcessingType.ProcessingType);
				result.Append(",");
			}

			AddInfo.ZA_AQISPremIdProcessType_Hidden = new ZString(result.ToString()).TrimEndIncludingWhiteSpace(',');
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISPremisesIdAndProcessingType(Factory, AddInfo.JobDeclaration);
		}
	}
}
