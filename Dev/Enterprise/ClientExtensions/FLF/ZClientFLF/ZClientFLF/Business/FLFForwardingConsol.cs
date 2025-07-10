using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.FLF
{
	public class FLFForwardingConsol : ForwardingConsol
	{
		#region Constructors and Type Overriding

		public FLFForwardingConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new FLFForwardingConsol New(BusinessObjectFactory factory)
		{
			return (FLFForwardingConsol)factory.New(typeof(FLFForwardingConsol));
		}

		#endregion

		#region Overrides

		#region DocumentSupporter
		public override DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fFLFForwardingConsolDocumentSupporter == null)
				{
					fFLFForwardingConsolDocumentSupporter = new FLFForwardingConsolDocumentSupporter(this);
				}

				return fFLFForwardingConsolDocumentSupporter;
			}
		}
		FLFForwardingConsolDocumentSupporter fFLFForwardingConsolDocumentSupporter;
		#endregion

		#endregion
	}
}
