using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public abstract class ManifestFormPerformanceTest<THeader, TContainer, TBill, TPack, TPackedItem, TABLEntryNum, TAsycudaPackedItemEntryNum> : BaseManifestFormPerformanceTest<THeader, TContainer, TBill, TPack, TPackedItem, TABLEntryNum, TAsycudaPackedItemEntryNum>
			where THeader : AsycudaManifestHeader
			where TContainer : AsycudaContainer
			where TBill : AsycudaBill
			where TPack : AsycudaPack
			where TPackedItem : AsycudaPackedItem
			where TABLEntryNum : ABLEntryNum
			where TAsycudaPackedItemEntryNum : AsycudaPackedItemEntryNum
	{
		#region Mark

		IDisposable Mark(string location, TextWriter writer, BusinessObjectFactory factory, string expectedDBHits, Func<TableHitCount, bool> isExemptedTable)
		{
			return new PerformanceMark(location, writer, factory, expectedDBHits, isExemptedTable);
		}

		class PerformanceMark : IDisposable
		{
			public PerformanceMark(string location, TextWriter writer, BusinessObjectFactory factory, string expectedDBHits, Func<TableHitCount, bool> isExemptedTable)
			{
				showError = writer == null;
				this.writer = writer ?? new StringWriter();
				databaseHitMark = new DatabaseHitMark(location, this.writer, factory, expectedDBHits, isExemptedTable);
			}

			public void Dispose()
			{
				databaseHitMark.Stop();
				if (showError)
				{
					if (databaseHitMark.HasError)
					{
						AssertMultilineASCIIEquals("DB Hints failed", databaseHitMark.expectedDBHits.Replace("Copy and paste this set of expected", ""), writer.ToString());
					}
				}
				else
				{
					writer.WriteLine("");
				}
			}

			readonly bool showError;
			readonly TextWriter writer;
			readonly DatabaseHitMark databaseHitMark;
		}

		class DatabaseHitMark
		{
			public DatabaseHitMark(string location, TextWriter writer, BusinessObjectFactory factory, string expectedDBHits, Func<TableHitCount, bool> isExemptedTable)
			{
				this.location = location;
				this.writer = writer;
				this.factory = factory;
				this.expectedDBHits = expectedDBHits;
				this.isExemptedTable = isExemptedTable;
				factory.ResetDatabaseLoadCount();
			}

			public void Stop()
			{
				var errorBuilder = new ZStringBuilder();
				var expectedElements = Regex.Split(expectedDBHits, "\r\n");
				var actualDBHitsBuilder = new ZStringBuilder();

				foreach (var hitCount in factory.TableSelects.Where(x => x.Value > 0).OrderBy(x => x.TableName))
				{
					var dbHit = string.Format("{0} ({1})", hitCount.TableName, hitCount.Value);
					actualDBHitsBuilder.Append(dbHit);
					if (HasFailedFetchHint(hitCount) && !isExemptedTable(hitCount))
					{
						var expectedHitPredicate = expectedElements.Where(e => e.StartsWith(hitCount.TableName + " ", StringComparison.InvariantCultureIgnoreCase));
						if (expectedHitPredicate.Any())
						{
							var expectedHit = expectedHitPredicate.First();
							var expectedCount = new ZString(expectedHit.Split(' ').Last()).KeepNumericCharacters();
							if (expectedCount == "" || int.Parse(expectedCount) < hitCount.Value)
							{
								errorBuilder.Append(dbHit + "  - expected only " + expectedCount);
							}
						}
						else
						{
							errorBuilder.Append(dbHit + " {did not expect any hits at all}");
						}
					}
				}
				int actualHits = factory.DatabaseLoadCount;
				if (!errorBuilder.IsEmpty)
				{
					writer.WriteLine(errorBuilder.ToStringWithNewLineBetweenAppends());
					writer.WriteLine("Copy and paste this set of expected hits into your expectations list:");
				}
				writer.WriteLine(location + " Actual hits : " + actualHits);
				writer.WriteLine(actualDBHitsBuilder.ToStringWithNewLineBetweenAppends());
			}

			public bool HasError
			{
				get;
				private set;
			}

			readonly TextWriter writer;
			readonly string location;
			readonly BusinessObjectFactory factory;
			public readonly string expectedDBHits;
			readonly Func<TableHitCount, bool> isExemptedTable;

			bool HasFailedFetchHint(TableHitCount tableSelect)
			{
				return tableSelect.Value > 6;
			}
		}

		#endregion

		[SnailTest]
		public void TestFetchHints()
		{
			//AssertFetchHints(new ZString[] { Core.Constants.CountryCodes.Singapore }, 200, 3, false, true);
			//AssertFetchHints(null, 100, 10, false, true);
			var numberOfBills = 10;
			var numberOfPacks = 10;
			var numberOfContainers = 10;
			var createContacts = false;
			// set writer if you want to output the result in one file:
			StringWriter writer = null; // new StringWriter();
			VoidParameterlessDelegate assertData = () =>
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				THeader manifestHeader;
				using (Mark(string.Format("Create - {0} Bills with {1} Packs - Country ({2}) - Container {3} - Create Contacts {4}", numberOfBills, numberOfPacks, CountryCode, numberOfContainers, createContacts), writer, factory, ExpectedCreateDBHits, IsExemptedTableForCreate))
				{
					var testData = new TestData(CountryCode, factory);
					manifestHeader = CreateFullyPopulatedObject(testData, factory, numberOfBills, numberOfPacks, numberOfContainers, createContacts);
				}
				using (Mark("Performance - Saving", writer, factory, ExpectedSavingDBHits, IsExemptedTableForSaving))
				{
					factory.Save();
				}
				RowFactory.ResetCacheAfterDbUpgrade();
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				using (Mark("Performance - Show Form", writer, factory, ExpectedShowFormDBHits, IsExemptedTableForShowForm))
				{
					manifestHeader = factory.Load<THeader>(manifestHeader.PK);
					using (UserIdleWorker.Suspend())
					using (ZForm form = GetForm(manifestHeader))
					{
						form.Show();
						form.Update();
					}
				}
				RowFactory.ResetCacheAfterDbUpgrade();
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				manifestHeader = factory.Load<THeader>(manifestHeader.PK);
				using (UserIdleWorker.Suspend())
				using (ZForm form = GetForm(manifestHeader))
				{
					form.Show();
					form.Update();
					//MessageBox.Show("Load");
					using (Mark("Performance - LoadChildEditableObjects", writer, factory, ExpectedLoadChildEditableObjectsDBHits, IsExemptedTableForLoadChildEditableObjects))
					{
						manifestHeader.LoadChildEditableObjects();
					}
				}
				RowFactory.ResetCacheAfterDbUpgrade();
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				manifestHeader = factory.Load<THeader>(manifestHeader.PK);
				using (UserIdleWorker.Suspend())
				using (ZForm form = GetForm(manifestHeader))
				{
					form.Show();
					form.Update();
					using (Mark("Performance - Form Validate All", writer, factory, ExpectedFormValidateAllDBHits, IsExemptedTableForFormValidateAll))
					{
						form.Menu.MenuItems.FindByText("&Validate All", true).PerformClick();
					}
				}
				RowFactory.ResetCacheAfterDbUpgrade();
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				manifestHeader = factory.Load<THeader>(manifestHeader.PK);
				using (UserIdleWorker.Suspend())
				using (ZForm form = GetForm(manifestHeader))
				{
					form.Show();
					form.Update();
					//MessageBox.Show("Light");
					using (Mark("Performance - Form Light Validation And Save", writer, factory, ExpectedFormLightValidateAndSaveDBHits, IsExemptedTableForFormLightValidateAndSave))
					{
						form.FireSaveButton();
					}
				}
				var sendManifestMenuName = string.Format("Send Manifest ({0})", manifestHeader.CountryName);
				var preSendManifestData = SetupDataForSendManifest(manifestHeader);
				RowFactory.ResetCacheAfterDbUpgrade();
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				manifestHeader = factory.Load<THeader>(manifestHeader.PK);
				using (UserIdleWorker.Suspend())
				using (ZForm form = GetForm(manifestHeader))
				{
					form.Show();
					form.Update();
					AsycudaMenu asycudaMenu;
					using (Mark("Performance - Show Menu", writer, factory, ExpectedShowMenuDBHits, IsExemptedTableForShowMenu))
					{
						asycudaMenu = (AsycudaMenu)((IFileMenuItemsProvider)form).MainMenu.MenuItems.FindByText("Manifest");
						asycudaMenu.OnPopup(EventArgs.Empty);
					}
					var sendManifestMenuItem = asycudaMenu.MenuItems.FindByText(sendManifestMenuName);
					//MessageBox.Show("Manifest");
					using (Mark("Performance - Send Manifest", writer, factory, ExpectedSendManifestDBHits, IsExemptedTableForSendManifest))
					{
						sendManifestMenuItem?.PerformClick();
					}
				}
				SetDataToPreSendManifest(preSendManifestData);
				RowFactory.ResetCacheAfterDbUpgrade();
				UniversalShipment shipment;
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				IShipmentDataContextManager manager = new AsycudaManifestHeaderDataContextManager();
				manifestHeader = factory.Load<THeader>(manifestHeader.PK);
				using (UserIdleWorker.Suspend())
				{
					using (Mark("Performance - Universal XML Export", writer, factory, ExpectedUniversalXmlExportDBHits, IsExemptedTableForUniversalXmlExport))
					{
						shipment = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, manifestHeader);
					}
				}
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.CodesMappedToTarget = true; // Required to import JobCosting
				dataContext.AddDataTarget(DataContextType.AsycudaManifest, manifestHeader.AMA_JobReference);
				shipment.WayBillNumber = "MKD3232";
				shipment.DataContext = dataContext;
				ZInt entryInstructionLink = 0;
				AddEntryDetails(shipment, CountryCode, ref entryInstructionLink);
				eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var objectFactory = new UniversalObjectFactory();
				factory = objectFactory.BOFactory;
				factory.RefreshEnabled = false;
				RowFactory.ResetCacheAfterDbUpgrade();
				using (UserIdleWorker.Suspend())
				{
					using (Mark("Performance - Universal XML Import Update", writer, factory, ExpectedUniversalXmlImportUpdateDBHits, IsExemptedTableForUniversalXmlImportUpdate))
					{
						AssertEquals("Universal XML Import", true, manager.UseIncomingShipmentData(shipment, new TestErrorLogger(), objectFactory));
						objectFactory.SaveForTesting();
					}
				}
				dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				dataContext.CodesMappedToTarget = true; // Required to import JobCosting
				dataContext.AddDataTarget(DataContextType.AsycudaManifest, null);
				shipment.WayBillNumber = "JKD44332";
				shipment.DataContext = dataContext;
				SetUpDataForUniversalXmlImportAdd(shipment);
				objectFactory = new UniversalObjectFactory();
				factory = objectFactory.BOFactory;
				factory.RefreshEnabled = false;
				RowFactory.ResetCacheAfterDbUpgrade();
				using (UserIdleWorker.Suspend())
				{
					using (Mark("Performance - Universal XML Import Add", writer, factory, ExpectedUniversalXmlImportAddDBHits, IsExemptedTableForUniversalXmlImportAdd))
					{
						AssertEquals("Universal XML Add", true, manager.UseIncomingShipmentData(shipment, new TestErrorLogger(), objectFactory));
						objectFactory.SaveForTesting();
					}
				}
				RowFactory.ResetCacheAfterDbUpgrade();
				factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				manifestHeader = factory.Load<THeader>(manifestHeader.PK);
				using (UserIdleWorker.Suspend())
				{
					using (Mark("Performance - Delete", writer, factory, ExpectedDeleteDBHits, IsExemptedTableForDelete))
					{
						manifestHeader.Delete();
					}
				}
				if (writer != null)
				{
					AssertMultilineASCIIEquals("", "", writer.GetStringBuilder().ToString());
				}
				Assert("All good", true);
			};
			if (Globals.IsUserInteractive)
			{
				CombineAssertions(assertData);
			}
			else
			{
				assertData();
			}
		}

		void SetDataToPreSendManifest(Dictionary<BusinessObject, Dictionary<ZString, IZType>> preSendManifestData)
		{
			foreach (var pair in preSendManifestData)
			{
				var bizObj = pair.Key;
				foreach (var data in pair.Value)
				{
					if (data.Key == CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
					{
						if (bizObj is AsycudaPackedItem packedItem)
						{
							AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<TAsycudaPackedItemEntryNum>(bizObj, data.Value.ToString(), packedItem.CountryCode);
						}
						else if (bizObj is AsycudaBill bill)
						{
							AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<TABLEntryNum>(bizObj, data.Value.ToString(), bill.CountryCode);
						}
						else if (bizObj is AsycudaManifestHeader header)
						{
							AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<CusEntryNumber>(bizObj, data.Value.ToString(), header.AMA_RN_NKCountry);
						}
						else
						{
							AsycudaEntryNumberCreator.CreateOrUpdateRegistrationNumber<CusEntryNumber>(bizObj, data.Value.ToString(), ZString.Empty);
						}
					}
					else
					{
						bizObj[data.Key] = data.Value;
					}
				}
			}
			preSendManifestData.Keys.First().Factory.Save();
		}

		Dictionary<BusinessObject, Dictionary<ZString, IZType>> SetupDataForSendManifest(THeader manifestHeader)
		{
			var dictionary = new Dictionary<BusinessObject, Dictionary<ZString, IZType>>();
			AddAsycudaRegistration<CusEntryNumber>(dictionary, manifestHeader);
			foreach (TBill bill in manifestHeader.Bills)
			{
				AddAsycudaRegistration<TABLEntryNum>(dictionary, bill);
				foreach (AsycudaPack pack in bill.Packs)
				{
					var packedItem = pack.PackedItemForTesting();
					if (packedItem != null)
					{
						AddAsycudaRegistration<TAsycudaPackedItemEntryNum>(dictionary, packedItem);
					}
				}
			}
			dictionary.Keys.First().Factory.Save();
			return dictionary;
		}

		static void AddAsycudaRegistration<T>(Dictionary<BusinessObject, Dictionary<ZString, IZType>> dictionary, BusinessObject bo)
		where T : CusEntryNumber
		{
			var data = new Dictionary<ZString, IZType>();
			var asycudaRegistration = CusEntryNumber.Load<T>(bo, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, "");
			if (asycudaRegistration != null)
			{
				data.Add(CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, asycudaRegistration.CE_EntryNum);
				asycudaRegistration.CE_EntryNum = ZString.Empty;
			}
			dictionary.Add(bo, data);
		}

		void AddEntryDetails(UniversalShipment shipment, ZString country, ref ZInt entryInstructionLink)
		{
			// This is only needed as Writer and Reader don't currently use the same schema
			var addInfoCollection = shipment.AddInfoCollection;
			var link = entryInstructionLink;
			shipment.SetEntryHeaderCollection(() => new List<UniversalCustoms.EntryHeader>(new[]
			{
				new UniversalCustoms.EntryHeader()
				{
					Type = new EntryType() { Code = country },
					EntryInstructionLink = link,
					EntryNumberCollection = CreateEntryNumberCollection(shipment.EntryNumberCollection)
				}
			}));
			shipment.SetEntryInstructionCollection(() => new List<UniversalCustoms.EntryInstruction>(new[]
			{
				new UniversalCustoms.EntryInstruction()
				{
					Link = link,
					CustomsOffice = new CodeDescriptionPair5Char() { Code = addInfoCollection.GetZStringValue(AddInfoConstants.Header.CustomsOffice) },
					LocationAtClearance = new CodeDescriptionPair35Char() { Code = addInfoCollection.GetZStringValue(AddInfoConstants.Bill.ABL_LocationOfGoods) },
					AddInfoCollection = addInfoCollection,
					OrganizationAddressCollection = shipment.OrganizationAddressCollection
				}
			}));
			if (shipment.SubShipmentCollection != null)
			{
				foreach (var subShipment in shipment.SubShipmentCollection)
				{
					entryInstructionLink++;
					AddEntryDetails(subShipment, country, ref entryInstructionLink);
				}
			}
		}

		List<UniversalCustoms.EntryNumber> CreateEntryNumberCollection(List<EntryNumber> entryNumberCollection)
		{
			List<UniversalCustoms.EntryNumber> result = null;
			if (entryNumberCollection != null)
			{
				result = new List<UniversalCustoms.EntryNumber>();
				foreach (var entryNumber in entryNumberCollection)
				{
					result.Add(new UniversalCustoms.EntryNumber()
					{
						Type = entryNumber.Type,
						EntryStatus = entryNumber.EntryStatus,
						Number = entryNumber.Number,
						IssueDate = entryNumber.IssueDate,
						EntryIsSystemGenerated = entryNumber.EntryIsSystemGenerated
					});
				}
			}
			return result;
		}

		void SetUpDataForUniversalXmlImportAdd(UniversalShipment shipment)
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataTarget(DataContextType.AsycudaBill, null);

			shipment.SubShipmentCollection.Add(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				WayBillNumber = "HBNEW" + ZDateTime.Now.ToString(DateFormat),
				DataContext = dataContext
			});
		}

		bool IsExemptedTableForCreate(TableHitCount tableSelect)
		{
			return (tableSelect.TableName == ZZRefCusCodeListCombinedSchema.Constants.TableName && tableSelect.Value < 9);
		}

		bool IsExemptedTableForSaving(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForShowForm(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForLoadChildEditableObjects(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForFormValidateAll(TableHitCount tableSelect)
		{
			return (tableSelect.TableName == ZZRefCusCodeListCombinedSchema.Constants.TableName && tableSelect.Value < 23); // TODO needs to add fetch hints
		}

		bool IsExemptedTableForFormLightValidateAndSave(TableHitCount tableSelect)
		{
			return (tableSelect.TableName == ZZRefCusCodeListCombinedSchema.Constants.TableName && tableSelect.Value < 23) // TODO needs to add fetch hints
				|| (tableSelect.TableName == StmNoteSchema.Constants.TableName) // TODO needs to add fetch hints
				|| (tableSelect.TableName == StmUniversalCopySchema.Constants.TableName);
		}

		bool IsExemptedTableForShowMenu(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForSendManifest(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForUniversalXmlExport(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForUniversalXmlImportUpdate(TableHitCount tableSelect)
		{
			return (tableSelect.TableName == AsycudaContainerBillOrPackageLinkSchema.Constants.TableName && tableSelect.Value < 20) // TODO needs to add fetch hints
				|| (tableSelect.TableName == AsycudaPackedItemSchema.Constants.TableName && tableSelect.Value < 101) // TODO needs to add fetch hints
				|| (tableSelect.TableName == CusEntryNumSchema.Constants.TableName && tableSelect.Value < 103) // TODO needs to add fetch hints
				|| (tableSelect.TableName == GenAddOnColumnSchema.Constants.TableName && tableSelect.Value < 103) // TODO needs to add fetch hints
				|| (tableSelect.TableName == StmNoteSchema.Constants.TableName && tableSelect.Value < 202) // TODO needs to add fetch hints
				|| (tableSelect.TableName == StmUniversalCopySchema.Constants.TableName);
		}

		bool IsExemptedTableForUniversalXmlImportAdd(TableHitCount tableSelect)
		{
			return false;
		}

		bool IsExemptedTableForDelete(TableHitCount tableSelect)
		{
			return (tableSelect.TableName == StmNoteSchema.Constants.TableName && tableSelect.Value < 8) ||
				tableSelect.TableName == StmUniversalCopySchema.Constants.TableName;
		}

		string ExpectedCreateDBHits => @"OrgAddress (2)
OrgAddressCapability (1)
OrgCompanyData (1)
OrgHeader (1)
OrgMiscServ (1)
RefCountry (1)
ZZRefCusCodeListAttributeCombined (1)
ZZRefCusCodeListCombined (8)";

		string ExpectedSavingDBHits => @"OrgAddress (1)
OrgHeader (1)
ProcessTaskTemplate (1)";

		string ExpectedShowFormDBHits => @"AsycudaBill (2)
AsycudaBill (1)
AsycudaManifestHeader (1)
AsycudaPack (1)
AsycudaPackedItem (1)
CusEntryNum (3)
GenAddOnColumn (2)
OrgAddress (3)
OrgHeader (2)
ZZRefCarrierCombined (2)
ZZRefCusCodeListAttributeCombined (1)
ZZRefCusCodeListCombined (3)";

		string ExpectedLoadChildEditableObjectsDBHits => @"AsycudaBill (2)
AsycudaContainer (1)
AsycudaPack (1)
AsycudaPackedItem (1)
CusEntryNum (2)
CusPerson (1)
EDIMessage (2)
GenPivot (1)
ProcessTasks (1)
UNDGDataItem (1)";

		string ExpectedFormValidateAllDBHits => @"AsycudaBill (1)
AsycudaBill (1)
AsycudaContainer (1)
AsycudaPack (1)
AsycudaContainerBillOrPackageLink (2)
AsycudaPackedItem (1)
CusEntryNum (2)
CusPerson (1)
EDIMessage (2)
GenAddOnColumn (3)
GenCustomAddOnRuleAck (1)
GenCustomAddOnValue (1)
GenPivot (1)
OrgAddress (1)
OrgAddressCapability (1)
OrgCompanyData (1)
OrgHeader (1)
ProcessTasks (1)
RefCountry (3)
RefCurrency (1)
RefPacks (1)
RefPackType (1)
RefUNLOCO (3)
RefVessel (1)
StmNote (1)
UNDGDataItem (1)
ZZRefCusCodeListAttributeCombined (4)
ZZRefCusCodeListCombined ()
ZZRefCusMapCombined (2)";

		string ExpectedFormLightValidateAndSaveDBHits => @"AsycudaBill (1)
AsycudaBill (1)
AsycudaContainer (1)
AsycudaPack (1)
AsycudaContainerBillOrPackageLink (2)
AsycudaPackedItem (1)
CusEntryNum (2)
CusPerson (1)
EDIMessage (1)
GenAddOnColumn (3)
GenCustomAddOnRuleAck (1)
GenCustomAddOnValue (1)
GenPivot (1)
OrgAddress (1)
OrgAddressCapability (1)
OrgCompanyData (1)
OrgHeader (1)
ProcessTasks (1)
RefCountry (3)
RefCurrency (1)
RefPacks (1)
RefPackType (1)
RefUNLOCO (3)
RefVessel (1)
StmNote ()
StmUniversalCopy ()
UNDGDataItem (1)
ZZRefCusCodeListAttributeCombined (4)
ZZRefCusCodeListCombined ()
ZZRefCusMapCombined (2)";

		string ExpectedShowMenuDBHits => @"AsycudaBill (1)
AsycudaBill (1)
AsycudaPack (1)
AsycudaPackedItem (1)
CusEntryNum (2)
RefCountry (1)
ZZRefCusCodeListAttributeCombined (1)
ZZRefCusCodeListCombined (10)";

		string ExpectedSendManifestDBHits => @"AsycudaContainer (1)
AsycudaContainerBillOrPackageLink (2)
CusPerson (1)
EDIMessage (2)
GenAddOnColumn (3)
GenPivot (1)
OrgAddress (1)
OrgHeader (1)
ProcessTasks (1)
RefCountry (2)
RefCurrency (1)
RefPacks (2)
RefPackType (1)
RefUNLOCO (3)
RefVessel (1)
UNDGDataItem (1)
ZZRefCusCodeListAttributeCombined (3)
ZZRefCusCodeListCombined (20)
ZZRefCusMapCombined (2)";

		string ExpectedUniversalXmlExportDBHits => @"AsycudaBill (1)
AsycudaBill (1)
AsycudaContainer (1)
AsycudaPack (1)
AsycudaContainerBillOrPackageLink (1)
AsycudaPackedItem (1)
CusEntryNum (2)
OrgAddress (3)
OrgAddressCapability (2)
OrgCusCode (4)
OrgHeader (1)
ProcessTasks (1)
RefCurrency (1)
RefPacks (1)
RefPackType (1)
RefUNLOCO (1)
RefVessel (1)
StmNote (3)
UNDGDataItem (1)
ZZRefCusCodeListCombined (2)";

		string ExpectedUniversalXmlImportUpdateDBHits => @"AsycudaBill (1)
AsycudaBill (1)
AsycudaContainer (1)
AsycudaManifestHeader (1)
AsycudaPack (1)
AsycudaContainerBillOrPackageLink (19)
AsycudaPackedItem (100)
CusEntryNum (102)
EDIMessage (9)
GenAddOnColumn (102)
GenPivot (1)
OrgAddress (3)
OrgHeader (3)
ProcessTaskTemplate (1)
StmNote (201)
StmUniversalCopy (512)
UNDGDataItem (1)
ZZRefCusCodeListCombined (1)";

		string ExpectedUniversalXmlImportAddDBHits => @"AsycudaManifestHeader (1)
OrgAddress (3)
OrgAddressCapability (1)
OrgHeader (3)
ProcessTaskTemplate (1)
ZZRefCusCodeListAttributeCombined (1)
ZZRefCusCodeListCombined (3)";

		string ExpectedDeleteDBHits => @"AsycudaBill (1)
AsycudaBill (1)
AsycudaContainer (1)
AsycudaPack (1)
AsycudaContainerBillOrPackageLink (11)
AsycudaPackedItem (1)
CusEntryNum (1)
CusPerson (1)
EDIInterchange (1)
EDIMessage (4)
GenAddOnColumn (1)
GenPivot (2)
JobDocAddress (1)
OrgAddress (1)
OrgHeader (1)
ProcessHeader (1)
ProcessTasks (1)
ProcessTaskTemplate (1)
StmNote (7)
StmUniversalCopy (20)
UNDGDataItem (1)";
	}
}
