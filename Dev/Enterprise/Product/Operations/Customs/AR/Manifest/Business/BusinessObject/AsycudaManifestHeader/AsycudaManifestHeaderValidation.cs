using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public const string VV = "08";

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateRegistrationNumber();
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AMA_NatureInfo);
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();

			if (Parent.AMA_TransportMode == Core.Constants.TransportModes.Sea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_CustomsOfficeInfo);
			}
		}

		protected override void CheckRegistrationDate()
		{
			base.CheckRegistrationDate();

			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.RegistrationDateInfo);
			}
		}

		public void ValidateRegistrationNumber()
		{
			ValidateCalculatedProperty(Parent.RegistrationNumberInfo);
		}

		protected void CheckRegistrationNumber()
		{
			if (Parent.IsSea)
			{
				var regNo = (ZString)Parent.RegistrationNumberInfo.Value;
				var regNoInfo = Parent.RegistrationNumberInfo;
				var regNoInfoLength = regNoInfo.MaxLength;

				if (!regNo.IsEmpty)
				{
					if (regNo.Length != regNoInfoLength)
					{
						regNoInfo.AddMessageError(ResString.GetMultilingualString("F696D560-C494-4625-89DF-627F80A024CE", "The length should be {0}", regNoInfoLength));
					}
					else if (regNo.Left(4) != Parent.AMA_SystemCreateTimeUtc.Year.ToString() || regNo.SubstringSafe(4, 2) != VV)
					{
						regNoInfo.AddMessageError(ResString.GetMultilingualString("AB6DA55C-2635-4DC5-BF43-13793A0637BD", "Incorrect ID format, the correct Format is YYYYVVNNNNNNNNND (YYYY- Year, VV - 08, N - Sequential Number, D - Verification digit)"));
					}
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(regNoInfo);
				}
			}
		}

		protected override void CheckAMA_OA_ShippingAgent()
		{
			base.CheckAMA_OA_ShippingAgent();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_ShippingAgentInfo);
			}
		}

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
			}
			else
			{
				base.CheckAMA_OA_CarrierMandatory();
			}
		}
	}
}
