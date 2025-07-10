using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class IAP4Wrapper : IAP4
	{
		public IAP4Wrapper(AsycudaBill bill, ZString action, ZString reason, ZString amendType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.action = action;
			this.reason = reason;
			this.amendType = amendType;
		}

		readonly AsycudaBill bill;
		readonly ZString action;
		readonly ZString reason;
		readonly ZString amendType;

		IReadOnlyCollection<IALX1> IAP4.ALX1 => new IALX1[] { new IALX1Wrapper(bill, action, reason, amendType) };

		IPortInformation IAP4.P4 => p4 ?? (action == SEA309Constants.NewManifest ? p4 = new P4Wrapper(bill.Header) : null);
		IPortInformation p4;
	}

	internal class P4Wrapper : IPortInformation
	{
		readonly AsycudaManifestHeader header;

		public P4Wrapper(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
		}

		string IPortInformation.LocationIdentifier => header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? header.AMA_CustomsDischargePort : header.AMA_CustomsLoadPort;

		string IPortInformation.Date => header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? header.AMA_E_DEP.ToString("yyyyMMdd") : header.AMA_E_ARV.ToString("yyyyMMdd");
	}

	internal class IALX1Wrapper : IALX1
	{
		readonly AsycudaBill bill;
		readonly ZString action;
		readonly ZString reason;
		readonly ZString amendType;

		public IALX1Wrapper(AsycudaBill bill, ZString action, ZString reason, ZString amendType)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.action = action;
			this.reason = reason;
			this.amendType = amendType;
		}

		string IALX1.LXAssignedNumber => SEA309Constants.IncludedGroups;

		IM13ManifestAmendmentDetails IALX1.M13 => action != SEA309Constants.NewManifest ? (m13 = new M13Wrapper(bill, amendType, reason)) : null;
		protected IM13ManifestAmendmentDetails m13;

		IM11ManifestBillLadingDetails IALX1.M11 => action == SEA309Constants.NewManifest ? (m11 = new M11Wrapper(bill)) : null;
		protected IM11ManifestBillLadingDetails m11;

		IReadOnlyCollection<IN9ReferenceIdentification> IALX1.N9 => new IN9ReferenceIdentification[2] { new N9Wrappers(bill, false), new N9Wrappers(bill, true) };

		IReadOnlyCollection<IAN1408> IALX1.AN1408 => new List<IAN1408> { new AN1408Wrappers(bill, Parties.ConsigneeCode), new AN1408Wrappers(bill, Parties.ShipperCode), new AN1408Wrappers(bill, Parties.NotifyCode) };

		IReadOnlyCollection<IAVID> IALX1.AVID
		{
			get
			{
				var result = new List<IAVID>();

				foreach (AsycudaContainer container in bill.Header.Containers)
				{
					result.Add(new AVIDWrappers(container, bill));
				}

				return result.ToArray();
			}
		}
	}
}
