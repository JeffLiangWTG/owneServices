using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent) : base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckF50HasATransportMeans();
		}

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();

			if (Parent.Bill?.Header is AsycudaManifestHeader header)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.APA_PackUQInfo,
					header.SpecificCircumstanceIndicatorInfo,
					new IZType[]
					{
						(ZString)EUICS2SpecificCircumstanceList.Codes.F22,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F26,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F50
					});
			}

			ValidateAPA_MarksAndNumbers();
		}

		protected override bool IsVolumeRequired() => false;

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();

			var message = Res.GetString("B57C16E8-4D6A-46ED-B0FF-935CB4C47131", "You have not entered Marks and Numbers (on Pack)");

			var parent = Parent;
			var packUQ = parent.APA_PackUQ;

			if (!packUQ.IsEmpty && !packUQ.In(new ZString[] { "VQ", "VG", "VL", "VY", "VR", "VS", "VO", "NE", "NF", "NG" }))
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.APA_MarksAndNumbersInfo, parent.APA_PackUQInfo, packUQ, message);
			}

			ValidationHelper.CheckMaxLength(parent.APA_MarksAndNumbersInfo, 512);
		}

		void CheckF50HasATransportMeans()
		{
			var message = Res.GetString("DE9F2FDD-AB2A-4A83-960D-8742DD01DD3D", "You have not entered Passive Border Transport Information.");
			Parent.RemoveRowMessageError(message);

			if (Parent.Bill is { Header.SpecificCircumstanceIndicator: var indicator } && indicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F50) && Parent.AsycudaTransportMeans.Count == 0)
			{
				Parent.AddRowMessageError(message);
			}
		}

		protected override bool IsPackQtyRequired() => !IsZeroPackQtyAllowed;

		protected override void CheckPackQtyIsZero()
		{
			Parent.APA_PackQtyInfo.AddMessageError(Res.GetString("256578F6-AB2E-412C-84C8-7341096CDA9B", "Quantity (on Pack) can only be 0, if another pack record with the same 'Marks and Numbers (on Pack)' and 'Quantity (on Pack) > 0 has been entered."));
		}

		protected override bool IsWeightRequired() => Parent.APA_PackQty != 0;

		bool IsZeroPackQtyAllowed => !Parent.APA_MarksAndNumbers.IsEmpty && Parent.Bill is { Packs: { } packs } && packs.Cast<AsycudaPack>().Any(x => x.APA_MarksAndNumbers == Parent.APA_MarksAndNumbers && x.APA_PackQty > 0);
	}
}
