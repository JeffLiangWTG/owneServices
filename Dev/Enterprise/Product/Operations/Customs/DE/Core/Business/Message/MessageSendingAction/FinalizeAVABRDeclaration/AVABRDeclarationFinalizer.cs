using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business;

public class AVABRDeclarationFinalizer
{
	public AVABRDeclarationFinalizer(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}

	readonly CusEntryHeader entryHeader;

	public void FinalizeDeclaration()
	{
		entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX8;
		entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());

		UpdateLinkedWarehouseOrder();
	}

	void UpdateLinkedWarehouseOrder()
	{
		var mrn = entryHeader.MovementReferenceNumber;
		var declaration = entryHeader.Declaration;
		var factory = declaration.Factory;

		var pivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, declaration.PK).AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, declaration.TablePrefix);
		var whsDocketJobPivot = factory.LoadTop1<IWhsDocketJobPivot>(pivotQuery);

		if (whsDocketJobPivot != null)
		{
			var linesQuery = new ZQuery(WhsDocketLineSchema.WE_WD, whsDocketJobPivot.WV_WD_Docket);
			var lines = factory.Load<IWhsDocketLine>(linesQuery);
			var linePks = lines.Select(x => x.PK);

			var whsAttributes = factory.Load<IWhsBondedWarehouseAttribute>(new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, linePks));
			foreach (var whsAttribute in whsAttributes)
			{
				whsAttribute.WB_EntryKey = mrn;
			}
		}
	}
}
