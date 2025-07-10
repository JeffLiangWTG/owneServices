using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class PreviousDocument : EU.H7.Business.PreviousDocument
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

	protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

	[List(nameof(Lookups) + "." + nameof(PreviousDocumentLookups.CodeList))]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set => base.CSI_Code = value;
	}

	protected override ZString GetPreviousDocumentDescription() => (Lookups.CodeList is CodeDescriptionPairList codeDescriptionPairList) ? codeDescriptionPairList.GetDescriptionFromCode(CSI_Code) : string.Empty;
}
