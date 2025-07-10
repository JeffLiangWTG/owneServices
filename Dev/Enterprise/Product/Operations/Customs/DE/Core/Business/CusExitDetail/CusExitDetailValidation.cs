using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitDetailValidation : EU.Business.CusExitDetailValidation
	{
		public CusExitDetailValidation(AutoCusExitDetail parent) : base(parent)
		{
		}

		protected new CusExitDetail Parent => (CusExitDetail)base.Parent;

		protected override bool IsStatusMandatory => false;

		protected override void CheckCED_CustomsOffice()
		{
			base.CheckCED_CustomsOffice();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CED_CustomsOfficeInfo);
		}

		protected override void CheckCED_MovementReferenceNumber()
		{
			base.CheckCED_MovementReferenceNumber();

			ValidateCED_MovementReferenceNumberFormat();
			ValidateCED_MovementReferenceNumberUnique();
		}

		void ValidateCED_MovementReferenceNumberFormat()
		{
			var parent = Parent;
			var mrnError = MRNFormatValidator.CheckMRNFormat(parent.CED_MovementReferenceNumber, parent.Factory, ZString.Empty);
			if (!mrnError.IsEmpty)
			{
				parent.CED_MovementReferenceNumberInfo.AddMessageError(mrnError);
			}
		}

		protected override void CheckCED_ExitDate()
		{
			base.CheckCED_ExitDate();

			var parent = Parent;
			if (StatusWithMandatoryExitDateAndTransportID.Contains(parent.CED_Status))
			{
				MandatoryValidation.WarnIfNotEntered(parent.CED_ExitDateInfo);
			}
		}

		protected override void CheckCED_TransportID()
		{
			base.CheckCED_TransportID();

			var parent = Parent;
			if (StatusWithMandatoryExitDateAndTransportID.Contains(parent.CED_Status))
			{
				MandatoryValidation.WarnIfNotEntered(parent.CED_TransportIDInfo);
			}
		}

		void ValidateCED_MovementReferenceNumberUnique()
		{
			var parent = Parent;
			var mrn = parent.CED_MovementReferenceNumber;
			if (!mrn.IsEmpty &&
				!StatusWithDuplicateMRN.Contains(parent.CED_Status) &&
				(parent.Header?.CusExitDetails.Cast<CusExitDetail>().Any(x => x.CED_MovementReferenceNumber == mrn && x.PK != parent.PK && !StatusWithDuplicateMRN.Contains(x.CED_Status)) ?? false))
			{
				parent.CED_MovementReferenceNumberInfo.AddWarning(Res.GetString("E9E3D589-F38A-4D65-A83F-A710CB1D7371", "MRN already exists on another Movement in this Exit Summary Job."));
			}
		}

		protected override void CheckCED_OA_Carrier()
		{
			base.CheckCED_OA_Carrier();

			var parent = Parent;
			var targetInfo = parent.CED_OA_CarrierInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (parent.Carrier is OrgAddress carrier)
			{
				var eoriCodeMissing = carrier.Header.GetEoriDetails().IsEmpty;
				var eoriBranchMissing = carrier.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).IsEmpty;
				if (eoriCodeMissing || eoriBranchMissing)
				{
					targetInfo.AddMessageError(Res.GetString("30E5B01E-1E94-4C52-ADD1-9DA7101D5D49", "Carrier is missing {0}", EORIHelper.GetMissingEoriMessage(eoriCodeMissing, eoriBranchMissing)));
				}
			}
		}

		ImmutableHashSet<string> StatusWithDuplicateMRN => Parent.Factory.GetCachedValue((NoResString)"Enterprise.Customs.DE.Business.CusExitDetailValidation | StatusWithDuplicateMRN", // Cache Key
			() => ImmutableHashSet.Create(
				UniversalReferenceConstants.CusExitDetailStatus._371,
				UniversalReferenceConstants.CusExitDetailStatus._372
			)
		);

		ImmutableHashSet<string> StatusWithMandatoryExitDateAndTransportID => Parent.Factory.GetCachedValue("Enterprise.Customs.DE.Business.CusExitDetailValidation|StatusWithMandatoryExitDate", // Cache Key
			() => ImmutableHashSet.Create(
				UniversalReferenceConstants.CusExitDetailStatus._310,
				UniversalReferenceConstants.CusExitDetailStatus._342,
				UniversalReferenceConstants.CusExitDetailStatus._353
			)
		);
	}
}
