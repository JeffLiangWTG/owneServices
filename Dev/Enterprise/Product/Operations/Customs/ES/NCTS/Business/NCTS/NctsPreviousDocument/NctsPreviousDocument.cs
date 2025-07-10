using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsPreviousDocument : EU.NCTS.Business.NctsPreviousDocument
	{
		public NctsPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument.Schema
		{
			public const int LineNoMaxLength = 5;
		}

		[MaxLength(Schema.LineNoMaxLength)]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		public new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public new EU.NCTS.Business.NctsPreviousDocumentLookups Lookups
		{
			get
			{
				if (IsPhase5)
				{
					return (EU.NCTS.Business.NctsPreviousDocumentPhase5Lookups)base.Lookups;
				}
				else
				{
					return (NctsPreviousDocumentPhase4Lookups)base.Lookups;
				}
			}
		}

		#region Implementation

		protected override CusSupportingInfoLookups GetNewPhase4Lookups() => new NctsPreviousDocumentPhase4Lookups(this);

		public new NctsPreviousDocumentValidation Validation => (NctsPreviousDocumentValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewPhase5Validation() => new NctsPreviousDocumentValidation(this);

		protected override CusSupportingInfoValidation GetNewPhase4Validation() => new NctsPreviousDocumentValidation(this);

		#endregion
	}
}
