using System.Collections.ObjectModel;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ReleaseItemsHandlerTest : TestCaseWithFactory
{
	public void TestHandleWithItemAtLineLevel()
	{
		var lineOne = entryHeader.AllEntryLines.AddNew();
		lineOne.CL_LineNumber = 1;
		var innerData = new Data
		{
			Informazione = new Collection<DataInformazione>
			{
				new DataInformazione { Livello = "S", NumeroArticolo = 1, Data = "01/01/2022", Numero = "1" }
			}
		};

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var handler = new ReleaseItemsHandler(adapter);
		handler.Handle(innerData);

		var clearanceCode = CusEntryNumber.Load(lineOne, "CLR", "IT", true);
		AssertNotNull("Clearance Code Entry", clearanceCode);
		AssertEquals("CLR Value", "1", clearanceCode.CE_EntryNum);
	}

	public void TestHandleWithItemAtHeaderLevel()
	{
		var innerData = new Data
		{
			Informazione = new Collection<DataInformazione>
			{
				new DataInformazione { Livello = "S", Data = "01/01/2022", Numero = "1" }
			}
		};

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var handler = new ReleaseItemsHandler(adapter);
		handler.Handle(innerData);

		var clearanceCode = CusEntryNumber.Load(entryHeader, "CLR", "IT", true);
		AssertNotNull("Clearance Code Entry", clearanceCode);
		AssertEquals("CLR Value", "1", clearanceCode.CE_EntryNum);
	}

	public void TestHandleWithNoItems()
	{
		var innerData = new Data
		{
			Informazione = new Collection<DataInformazione>
			{
				new DataInformazione { Livello = "P", Data = "01/01/2022", Numero = "1" }
			}
		};

		var adapter = new CusEntryHeaderCustomsLinkedObjectAdapter(entryHeader);
		var handler = new ReleaseItemsHandler(adapter);
		handler.Handle(innerData);
		var clearanceCode = CusEntryNumber.Load(entryHeader, "CLR", "IT", true);
		AssertNull("Clearance Code Entry", clearanceCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	CusEntryHeader entryHeader;
}
