using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.GVMS
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEmptyVehicle();
			ValidateRouteId();
			ValidateICSDeclarations();
			ValidateHaulierType();
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_JobReference()
		{
			base.CheckAMA_JobReference();

			var parent = Parent;
			if (!parent.GvmsCustomsReferenceCollection.Any() && !parent.GvmsTransitReferenceCollection.Any() && !parent.GvmsEidrAndOralReferenceCollection.Any())
			{
				parent.AMA_JobReferenceInfo.AddMessageError("There are no records in any of the three Customs References grids.");
			}
		}

		protected override void CheckAMA_CarrierCode()
		{
			base.CheckAMA_CarrierCode();

			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_CarrierCodeInfo);
		}

		protected override void CheckAMA_Nature()
		{
			base.CheckAMA_Nature();

			ValidateAMA_RL_NKPortOfLoading();
			ValidateAMA_RL_NKPortOfDischarge();
		}

		public void ValidateEmptyVehicle()
		{
			base.ValidateCalculatedProperty(Parent.EmptyVehicleInfo);
		}

		protected void CheckEmptyVehicle()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.EmptyVehicleInfo);
		}

		public void ValidateRouteId()
		{
			base.ValidateCalculatedProperty(Parent.RouteIdInfo);
		}

		protected void CheckRouteId()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.RouteIdInfo);

			var calculatedRoute = Parent.CalculatedRouteID;
			if (!Parent.RouteId.IsEmpty && !calculatedRoute.IsEmpty && !Parent.RouteId.EqualsIgnoringCase(calculatedRoute))
			{
				Parent.RouteIdInfo.AddMessageError(ZString.Format("CargoWise has calculated that the route code for {0}->{1} via carrier {2} should be {3}",
																		Parent.AMA_RL_NKPortOfLoading, Parent.AMA_RL_NKPortOfDischarge, Parent.AMA_CarrierCode, calculatedRoute));
			}
		}

		protected override void CheckAMA_Trailer1RegNo()
		{
			base.CheckAMA_Trailer1RegNo();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_Trailer1RegNoInfo);
		}

		protected override void CheckAMA_CustomsOffice()
		{
		}

		public void ValidateICSDeclarations()
		{
			var message = "Only one master S&S record is allowed. If multiple references are needed, detail one against each row in the S&S Reference column";

			foreach (var dec in Parent.CustomsReferences)
			{
				dec.RemoveRowMessageError(message);
			}

			var icsDecs = Parent.CustomsReferences.Where(x => x.CSI_Code == GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration && x.CSI_ReferenceNumber.IsEmpty).ToArray();
			if (icsDecs.Length > 1)
			{
				for (int i = 1; i < icsDecs.Length; i++)
				{
					icsDecs[i].AddRowMessageError(message);
				}
			}
		}

		public void ValidateHaulierType()
		{
			ValidateCalculatedProperty(Parent.HaulierTypeInfo);
		}

		protected void CheckHaulierType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.HaulierTypeInfo);
		}
	}
}
