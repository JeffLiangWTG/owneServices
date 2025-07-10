using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class SEA309Wrapper : ISea309SOManifest
	{
		public SEA309Wrapper(AsycudaBill bill, ZString action, ZString reason)
		{
			Bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			Action = action == MessageSubTypeCodes.Codes.Original ? SEA309Constants.NewManifest : SEA309Constants.OldManifest;
			Reason = reason;
			AmendType = action == MessageSubTypeCodes.Codes.Cancellation ? SEA309Constants.DeleteManifest : SEA309Constants.AmendmentManifest;
		}
		protected readonly AsycudaBill Bill;
		public ZString Action;
		public ZString Reason;
		public ZString AmendType;

		IHeader ISea309SOManifest.Header => header ?? (header = new HeaderWrapper(Bill.Header));
		protected IHeader header;

		IISAInterchangeControlHeader ISea309SOManifest.ISA => isa ?? (isa = new ISAWrapper());
		protected IISAInterchangeControlHeader isa;

		IGSFunctionalGroupHeader ISea309SOManifest.GS => gs ?? (gs = new GSWrapper());
		protected IGSFunctionalGroupHeader gs;

		ISTTransactionSetHeader ISea309SOManifest.ST => st ?? (st = new STWrapper());
		protected ISTTransactionSetHeader st;

		IM10ManifestIdentifyingInformation ISea309SOManifest.M10 => m10 ?? (m10 = new M10Wrapper(Bill.Header.AMA_Nature, Action));
		protected IM10ManifestIdentifyingInformation m10;

		IReadOnlyCollection<IAP4> ISea309SOManifest.AP4 => new IAP4[] { new IAP4Wrapper(Bill, Action, Reason, AmendType) };

		ISETransactionSetTrailer ISea309SOManifest.SE => se ?? (se = new SEWrapper());
		protected ISETransactionSetTrailer se;

		IGEFunctionalGroupTrailer ISea309SOManifest.GE => ge ?? (ge = new GEWrapper());
		protected IGEFunctionalGroupTrailer ge;

		IIEAInterchangeControlTrailer ISea309SOManifest.IEA => iea ?? (iea = new IEAWrapper());
		protected IIEAInterchangeControlTrailer iea;

		string ISea309SOManifest.Action => Action;
	}
}
