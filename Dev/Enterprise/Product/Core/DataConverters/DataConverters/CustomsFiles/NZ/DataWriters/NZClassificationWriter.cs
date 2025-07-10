using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.NZ;

namespace Enterprise.DataConverters.CustomsFiles.NZ
{
	public class ClassificationWriter : CustomsFiles.ClassificationWriter
	{
		#region Public Fields
		public ZString PartsOfTariffCode;
		public ZString ConcessionCode;
		public ZString PermitCode1;
		public ZString PermitNumber1;
		public ZString PermitCode2;
		public ZString PermitNumber2;
		public ZString PermitCode3;
		public ZString PermitNumber3;

		public ZString ProhibitedCode1;
		public ZString ProhibitedCode2;
		public ZString ProhibitedCode3;

		public ZString OtherInfoCode1;
		public ZString OtherInfoNumber1;
		public ZString OtherInfoCode2;
		public ZString OtherInfoNumber2;
		public ZString OtherInfoCode3;
		public ZString OtherInfoNumber3;
		#endregion

		public ClassificationWriter(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetClassificationType()
		{
			return ObjectFactory.GetType<ICusClassification>();
		}

		protected override void UpdateCountrySpecificData(Customs.Business.BaseCusClassification bizO)
		{
			var classification = (ICusClassification)bizO;
			classification.CC_PartsOfClassification = PartsOfTariffCode;
			classification.CC_ConcessionCode = ConcessionCode;

			if (!PermitCode1.IsEmpty)
			{
				classification.PermitCodes.AddOrUpdateExisting(PermitCode1, PermitNumber1);
			}
			if (!PermitCode2.IsEmpty)
			{
				classification.PermitCodes.AddOrUpdateExisting(PermitCode2, PermitNumber2);
			}
			if (!PermitCode3.IsEmpty)
			{
				classification.PermitCodes.AddOrUpdateExisting(PermitCode3, PermitNumber3);
			}

			if (!ProhibitedCode1.IsEmpty)
			{
				classification.ProhibitedCodes.AddOrUpdateExisting(ProhibitedCode1, "");
			}
			if (!ProhibitedCode2.IsEmpty)
			{
				classification.ProhibitedCodes.AddOrUpdateExisting(ProhibitedCode2, "");
			}
			if (!ProhibitedCode3.IsEmpty)
			{
				classification.ProhibitedCodes.AddOrUpdateExisting(ProhibitedCode3, "");
			}

			if (!OtherInfoCode1.IsEmpty)
			{
				classification.OtherInfos.AddOrUpdateExisting(OtherInfoCode1, OtherInfoNumber1);
			}
			if (!OtherInfoCode2.IsEmpty)
			{
				classification.OtherInfos.AddOrUpdateExisting(OtherInfoCode2, OtherInfoNumber2);
			}
			if (!OtherInfoCode3.IsEmpty)
			{
				classification.OtherInfos.AddOrUpdateExisting(OtherInfoCode3, OtherInfoNumber3);
			}
		}
	}
}
