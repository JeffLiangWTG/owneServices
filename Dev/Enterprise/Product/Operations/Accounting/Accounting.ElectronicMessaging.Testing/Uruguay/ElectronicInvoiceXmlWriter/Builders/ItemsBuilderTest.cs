using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	public class ItemsBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<ItemsBuilder>(new CFEBuilder().iTemsBuilder_constructorInitializedOnly);
		}

		public void TestItemsBuilder_WithTransactionInfo_TransactionType_INV()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			var vATTaxID = new TaxID { TaxCode = "EXEMPT", TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt } };
			SetPostingCollection(transactionInfo, 100, vATTaxID);
			var items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertItem_Det_Fact(items[0], 100, 100, 1);

			SetPostingCollection(transactionInfo, -100, vATTaxID);
			items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertItem_Det_Fact(items[0], 100, -100, -1);

			SetPostingCollection(transactionInfo, 200, new TaxID { TaxCode = UruguayConstants.IVA66, TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } });
			items = builder.BuildItems(transactionInfo);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], 194, 194, 1);
			AssertItem_Det_Fact(items[1], 6, 6, 1);

			SetPostingCollection(transactionInfo, -200, new TaxID { TaxCode = UruguayConstants.IVA66, TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } });
			items = builder.BuildItems(transactionInfo);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], 194, -194, -1);
			AssertItem_Det_Fact(items[1], 6, -6, -1);
		}

		public void TestItemsBuilder_WithTransactionInfo_TransactionType_CRD()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.CRD };
			var vATTaxID = new TaxID { TaxCode = "EXEMPT", TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt } };
			SetPostingCollection(transactionInfo, 100, vATTaxID);

			var items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertItem_Det_Fact(items[0], 100, -100, -1);

			SetPostingCollection(transactionInfo, -100, vATTaxID);

			items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertItem_Det_Fact(items[0], 100, 100, 1);

			SetPostingCollection(transactionInfo, 200, new TaxID { TaxCode = UruguayConstants.IVA66, TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } });
			items = builder.BuildItems(transactionInfo);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], 194, -194, -1);
			AssertItem_Det_Fact(items[1], 6, -6, -1);

			SetPostingCollection(transactionInfo, -200, new TaxID { TaxCode = UruguayConstants.IVA66, TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } });
			items = builder.BuildItems(transactionInfo);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], 194, 194, 1);
			AssertItem_Det_Fact(items[1], 6, 6, 1);
		}

		public void TestItemsBuilder_NullData()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;
			var items = builder.BuildItems(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));

			AssertNull(items);

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			var vATTaxID = new TaxID { TaxCode = "EXEMPT", TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt } };
			SetPostingCollection(transactionInfo, null, vATTaxID);

			items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertItem_Det_Fact(items[0], 0, 0, 1);

			SetPostingCollection(transactionInfo, null, vATTaxID);
			items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertItem_Det_Fact(items[0], 0, 0, 1);
		}

		void AssertItem_Det_Fact(Item_Det_Fact item_Det_Fact, ZDecimal expectedPrecioUnitario, ZDecimal expectedMontoItem, decimal expectedCantidad)
		{
			AssertEquals(nameof(Item_Det_Fact.UniMed), "N/A", item_Det_Fact.UniMed);
			AssertEquals(nameof(Item_Det_Fact.PrecioUnitario), expectedPrecioUnitario, item_Det_Fact.PrecioUnitario);
			AssertEquals(nameof(Item_Det_Fact.MontoItem), expectedMontoItem, item_Det_Fact.MontoItem);
			AssertEquals(nameof(Item_Det_Fact.Cantidad), expectedCantidad, item_Det_Fact.Cantidad);
		}

		void SetPostingCollection(TransactionInfo transactionInfo, decimal? amount, TaxID taxId)
		{
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = amount , VATTaxID = taxId  }
			});
		}

		public void TestItemsBuilder()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var description = "ALMACENAJE I.M. HASTA 27/05 HASU4200180 Destination AirWay Bill Free BUENOS AIRES RECOLETA ARGENTINA";
			var expectedNomItem = "ALMACENAJE I.M. HASTA 27/05 HASU4200180 Destination AirWay Bill Free BUENOS AIRE";

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 10, VATTaxID = new TaxID { TaxCode = "EXEMPT" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt } } , Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 20, VATTaxID = new TaxID { TaxCode = "NOTREPORT" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.NotReportable } } , Description = description   },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 30, VATTaxID = new TaxID { TaxCode = "CAPIVA" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.CapitalRated } } , Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 40, VATTaxID = new TaxID { TaxCode = UruguayConstants.IVA,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } } , Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 50, VATTaxID = new TaxID { TaxCode = UruguayConstants.IVA10 ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } } , Description = description  },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 60, VATTaxID = new TaxID { TaxCode = UruguayConstants.FREEIVA ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } } , Description = description  },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 70, VATTaxID = new TaxID { TaxCode = UruguayConstants.IVA66 ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } } , Description = description  },
			});

			var items = builder.BuildItems(transaction);

			AssertEquals(8, items.Length);
			AssertItem_Det_Fact(items[0], "1", Item_Det_FactIndFact.Item1, expectedNomItem);
			AssertItem_Det_Fact(items[1], "2", Item_Det_FactIndFact.Item1, expectedNomItem);
			AssertItem_Det_Fact(items[2], "3", Item_Det_FactIndFact.Item3, expectedNomItem);
			AssertItem_Det_Fact(items[3], "4", Item_Det_FactIndFact.Item3, expectedNomItem);
			AssertItem_Det_Fact(items[4], "5", Item_Det_FactIndFact.Item2, expectedNomItem);
			AssertItem_Det_Fact(items[5], "6", Item_Det_FactIndFact.Item10, expectedNomItem);
			AssertItem_Det_Fact(items[6], "7", Item_Det_FactIndFact.Item1, expectedNomItem);
			AssertItem_Det_Fact(items[7], "8", Item_Det_FactIndFact.Item3, expectedNomItem);
		}

		public void TestDscItemWhenTaxMessageIsNull()
		{
			AssertDscItem(taxMessage: null, expectedDscItemValue: null);
		}

		public void TestDscItemWhenTaxMessageEnglishTextIsNull()
		{
			var taxMessage = new TaxMessageID()
			{
				TaxMessageCode = "TEXMNUM01",
				Description = "POR FLETES DE EXPORTACION",
				EnglishTaxMessage = null,
			};
			AssertDscItem(taxMessage, null);
		}

		public void TestDscItemWhenTaxMessageEnglishTextIsEmpty()
		{
			var taxMessage = new TaxMessageID()
			{
				TaxMessageCode = "TEXMNUM01",
				Description = "POR FLETES DE EXPORTACION",
				EnglishTaxMessage = "",
			};
			AssertDscItem(taxMessage, null);
		}

		public void TestDscItemWhenTaxMessageEnglishTextIsNotEmpty()
		{
			var taxMessage = new TaxMessageID()
			{
				TaxMessageCode = "TEXMNUM01",
				Description = "",
				EnglishTaxMessage = "Servicio de Exportacion",
			};

			AssertDscItem(taxMessage, "{ Servicio de Exportacion }");
		}

		void AssertDscItem(TaxMessageID taxMessage, string expectedDscItemValue)
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 10, VATTaxID = new TaxID { TaxCode = "EXEMPT" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt } } , Description = "",  TaxMessageID = taxMessage }
			});

			var items = builder.BuildItems(transaction);

			AssertEquals("Postcondition: items Count", 1, items.Length);
			AssertEquals(expectedDscItemValue, items[0].DscItem);
		}

		public void TestItemsBuilderWithTaxCode_EXCLUDE()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var description = "ALMACENAJE I.M. HASTA 27/05 HASU4200180 Destination AirWay Bill Free BUENOS AIRES RECOLETA ARGENTINA";
			var expectedNomItem = "ALMACENAJE I.M. HASTA 27/05 HASU4200180 Destination AirWay Bill Free BUENOS AIRE";

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 30, VATTaxID = new TaxID { TaxCode = "EXCLUDE" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } }, Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = -30, VATTaxID = new TaxID { TaxCode = "EXCLUDE" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } }, Description = description }
			});

			var items = builder.BuildItems(transaction);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], "1", Item_Det_FactIndFact.Item6, expectedNomItem);
			AssertItem_Det_Fact(items[1], "2", Item_Det_FactIndFact.Item7, expectedNomItem);

			transaction.TransactionType = TransactionType.CRD;
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 30, VATTaxID = new TaxID { TaxCode = "EXCLUDE" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } }, Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = -30, VATTaxID = new TaxID { TaxCode = "EXCLUDE" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } }, Description = description }
			});

			items = builder.BuildItems(transaction);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], "1", Item_Det_FactIndFact.Item7, expectedNomItem);
			AssertItem_Det_Fact(items[1], "2", Item_Det_FactIndFact.Item6, expectedNomItem);
		}

		void AssertItem_Det_Fact(Item_Det_Fact item, string expectedNroLinDet, Item_Det_FactIndFact expectedFactIndFact, string expectedDescription)
		{
			AssertEquals(nameof(Item_Det_Fact.NroLinDet), expectedNroLinDet, item.NroLinDet);
			AssertEquals(nameof(Item_Det_Fact.IndFact), expectedFactIndFact, item.IndFact);
			AssertEquals(nameof(Item_Det_Fact.NomItem), expectedDescription, item.NomItem);
		}

		public void TestItemsBuilder_BadData()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var items = builder.BuildItems(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));

			AssertNull(items);

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			SetPostingCollection(70, null);
			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			SetPostingCollection(70, new TaxID { TaxCode = "EXCLUDE" });
			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			SetPostingCollection(null, new TaxID { TaxCode = "EXCLUDE", TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } });
			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			SetPostingCollection(70, new TaxID { TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } });
			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			SetPostingCollection(70, new TaxID { TaxCode = "XXX", TaxType = new CodeDescriptionPair() { Code = "XX" } });
			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			SetPostingCollection(null, null);
			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			SetPostingCollection(70, new TaxID { TaxCode = "EXCLUDE", TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } });
			items = builder.BuildItems(transactionInfo);

			AssertEquals(1, items.Length);
			AssertNullOrEmpty(nameof(Item_Det_Fact.NomItem), items[0].NomItem);

			void SetPostingCollection(decimal? amount, TaxID taxId)
			{
				transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>()
				{
					new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = amount , VATTaxID = taxId  }
				});
			}
		}

		public void TestItemsBuilderWithIva77AndIva396()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;

			var description = "ALMACENAJE I.M. HASTA 27/05 HASU4200180 Destination AirWay Bill Free BUENOS AIRES RECOLETA ARGENTINA";
			var expectedNomItem = "ALMACENAJE I.M. HASTA 27/05 HASU4200180 Destination AirWay Bill Free BUENOS AIRE";

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 10, VATTaxID = new TaxID { TaxCode = UruguayConstants.IVA77 ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } } , Description = description  },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = -20, VATTaxID = new TaxID { TaxCode = UruguayConstants.IVA396 ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Rated } } , Description = description  }
			});

			var items = builder.BuildItems(transaction);

			AssertEquals(4, items.Length);
			AssertItem_Det_Fact(items[0], "1", Item_Det_FactIndFact.Item1, expectedNomItem, 9.65m, 9.65m, 1);
			AssertItem_Det_Fact(items[1], "2", Item_Det_FactIndFact.Item3, expectedNomItem, 0.35m, 0.35m, 1);
			AssertItem_Det_Fact(items[2], "3", Item_Det_FactIndFact.Item1, expectedNomItem, 19.640m, -19.640m, -1);
			AssertItem_Det_Fact(items[3], "4", Item_Det_FactIndFact.Item3, expectedNomItem, 0.360m, -0.360m, -1);

			transaction.TransactionType = TransactionType.CRD;

			items = builder.BuildItems(transaction);

			AssertItem_Det_Fact(items[0], "1", Item_Det_FactIndFact.Item1, expectedNomItem, 9.65m, -9.65m, -1);
			AssertItem_Det_Fact(items[1], "2", Item_Det_FactIndFact.Item3, expectedNomItem, 0.35m, -0.35m, -1);
			AssertItem_Det_Fact(items[2], "3", Item_Det_FactIndFact.Item1, expectedNomItem, 19.640m, 19.640m, 1);
			AssertItem_Det_Fact(items[3], "4", Item_Det_FactIndFact.Item3, expectedNomItem, 0.360m, 0.360m, 1);

			void AssertItem_Det_Fact(Item_Det_Fact item, string expectedNroLinDet, Item_Det_FactIndFact expectedFactIndFact, string expectedDescription, decimal expectedPrecioUnitario, decimal expectedMontoItem, decimal expectedCantidad)
			{
				AssertEquals(nameof(Item_Det_Fact.NroLinDet), expectedNroLinDet, item.NroLinDet);
				AssertEquals(nameof(Item_Det_Fact.IndFact), expectedFactIndFact, item.IndFact);
				AssertEquals(nameof(Item_Det_Fact.NomItem), expectedDescription, item.NomItem);
				AssertEquals(nameof(Item_Det_Fact.UniMed), "N/A", item.UniMed);
				AssertEquals(nameof(Item_Det_Fact.PrecioUnitario), expectedPrecioUnitario, item.PrecioUnitario);
				AssertEquals(nameof(Item_Det_Fact.MontoItem), expectedMontoItem, item.MontoItem);
				AssertEquals(nameof(Item_Det_Fact.Cantidad), expectedCantidad, item.Cantidad);
			}
		}

		public void TestItemsBuilder_TaxTypeNotIsExcludedFromTheTaxBase()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			var description = "Almacenaje";

			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = -250, VATTaxID = new TaxID { TaxCode = "EXEMPT" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.Exempt } }, Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 300, VATTaxID = new TaxID { TaxCode = "NOTREPORT" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.NotReportable } }, Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = -450, VATTaxID = new TaxID { TaxCode = "CAPIVA" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.CapitalRated } }, Description = description }
			});

			var items = builder.BuildItems(transactionInfo);

			AssertEquals(3, items.Length);
			AssertItem_Det_Fact(items[0], 250, -250, -1);
			AssertItem_Det_Fact(items[1], 300, 300, 1);
			AssertItem_Det_Fact(items[2], 450, -450, -1);
		}

		public void TestItemsBuilder_TaxTypeIsExcludedFromTheTaxBase()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			var description = "Almacenaje";

			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal>()
			{
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = 100, VATTaxID = new TaxID { TaxCode = "EXCLUDE" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } }, Description = description },
				new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) { OSAmount = -200, VATTaxID = new TaxID { TaxCode = "EXCLUDE" ,TaxType = new CodeDescriptionPair() { Code = AccTaxRate.Types.ExcludedFromTheTaxBase } }, Description = description }
			});

			var items = builder.BuildItems(transactionInfo);

			AssertEquals(2, items.Length);
			AssertItem_Det_Fact(items[0], 100, 100, 1);
			AssertItem_Det_Fact(items[1], 200, 200, 1);
		}

		public void TestItemsBuilder_VATTaxIDAndTaxTypeWithValuesNull()
		{
			var builder = new ItemsBuilder() as IItemsBuilder;
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { TransactionType = TransactionType.INV };
			SetPostingCollection(transactionInfo, 10, null);

			var items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			var vATTaxID = new TaxID { TaxCode = "EXCLUDE", TaxType = null };
			SetPostingCollection(transactionInfo, 20, vATTaxID);

			items = builder.BuildItems(transactionInfo);

			AssertNull(items);

			vATTaxID = new TaxID { TaxCode = "EXCLUDE", TaxType = new CodeDescriptionPair() { Code = null } };
			SetPostingCollection(transactionInfo, 30, vATTaxID);

			items = builder.BuildItems(transactionInfo);

			AssertNull(items);
		}
	}
}
