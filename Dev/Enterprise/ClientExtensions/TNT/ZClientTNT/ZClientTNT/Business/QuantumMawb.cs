using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT
{
	public class QuantumMawb : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string MasterBillNum = "MasterBillNum";
			public const string DepartureDate = "DepartureDate";
			public const string PortOfLoading = "PortOfLoading";
			public const string PortOfDischarge = "PortOfDischarge";
			public const string FlightNumber = "FlightNumber";
			public const string LinkedConsolUniqueConsignRef = "LinkedConsolUniqueConsignRef";
			public const string ULDRequired = "ULDRequired";
		}

		public QuantumMawb(BusinessObjectFactory factory, QuantumSegment quantumSegment)
			: base(factory)
		{
			Argument.NotNull(quantumSegment, "Quantum Segment");
			fQuantumSegment = quantumSegment;
		}

		#region Properties

		#region Raw Properties - From File

		public string RawMasterBill
		{
			get { return fQuantumSegment.Consol.MasterBill; }
		}

		public string RawFlightNumber
		{
			get { return fQuantumSegment.Consol.FlightNumber; }
		}

		public string RawDepartureDate
		{
			get { return fQuantumSegment.Consol.DepartureDate.ToString(TNTConstants.DateFormat); }
		}

		public string RawPortOfLoading
		{
			get { return fQuantumSegment.Consol.PortOfLoading; }
		}

		public string RawPortOfDischarge
		{
			get { return fQuantumSegment.Consol.PortOfDischarge; }
		}

		#endregion

		#region Bound Properties

		#region MasterBillNum

		public virtual ZString MasterBillNum
		{
			get { return fQuantumSegment.Consol.MasterBill; }
		}

		public ZPropertyInfo MasterBillNumInfo
		{
			get { return GetZPropertyInfo(QuantumMawb.Schema.MasterBillNum); }
		}

		#endregion

		#region DepartureDate

		public virtual ZDateTime DepartureDate
		{
			get
			{
				return fQuantumSegment.Consol.DepartureDate;
			}
		}

		public ZPropertyInfo DepartureDateInfo
		{
			get { return GetZPropertyInfo(QuantumMawb.Schema.DepartureDate); }
		}

		#endregion

		#region PortOfLoading

		public virtual ZString PortOfLoading
		{
			get
			{
				return (ZString)TNTStringToBusinessObjectFieldConverter.Instance.ConvertRawStringToZTypeValue(
					typeof(ZString), ForeignKeyType.PortNK, Factory, fQuantumSegment.Consol.PortOfLoading, new NotificationBuffer(null));
			}
		}

		public ZPropertyInfo PortOfLoadingInfo
		{
			get { return GetZPropertyInfo(QuantumMawb.Schema.PortOfLoading); }
		}

		#endregion

		#region PortOfDischarge

		public virtual ZString PortOfDischarge
		{
			get
			{
				return (ZString)TNTStringToBusinessObjectFieldConverter.Instance.ConvertRawStringToZTypeValue(
					typeof(ZString), ForeignKeyType.PortNK, Factory, fQuantumSegment.Consol.PortOfDischarge, new NotificationBuffer(null));
			}
		}

		public ZPropertyInfo PortOfDischargeInfo
		{
			get { return GetZPropertyInfo(QuantumMawb.Schema.PortOfDischarge); }
		}

		#endregion

		#region FlightNumber

		public ZString FlightNumber
		{
			get { return fQuantumSegment.Consol.FlightNumber; }
		}

		public ZPropertyInfo FlightNumberInfo
		{
			get { return GetZPropertyInfo(QuantumMawb.Schema.FlightNumber); }
		}

		#endregion

		#region LinkedConsolUniqueConsignRef

		public ZString LinkedConsolUniqueConsignRef
		{
			get
			{
				ZString result = "";
				if (LinkedConsol != null)
				{
					result = LinkedConsol.JK_UniqueConsignRef;
				}
				return result;
			}
		}

		public ZPropertyInfo LinkedConsolUniqueConsignRefInfo
		{
			get { return GetZPropertyInfo(nameof(LinkedConsolUniqueConsignRef)); }
		}

		#endregion

		#region ULDRequired

		public ZBool ULDRequired
		{
			get { return fULDRequired; }
			set { SetNonPersistentPropertyValue(ULDRequiredInfo, ref fULDRequired, value); }
		}
		protected ZBool fULDRequired;

		public ZPropertyInfo ULDRequiredInfo
		{
			get { return GetZPropertyInfo(QuantumMawb.Schema.ULDRequired); }
		}

		#endregion

		#region RelatedShipmentsInFile

		public ZInt RelatedShipmentsInFile
		{
			get { return (ZInt)fQuantumSegment.Shipments.Length; }
		}

		public ZPropertyInfo RelatedShipmentsInFileInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedShipmentsInFile)); }
		}

		#endregion

		#endregion

		#region Search Properties

		#region SearchMasterBill

		ZString searchMasterBill;
		[MaxLength(ForwardingConsol.Schema.JK_MasterBillNumMaxLength)]
		public ZString SearchMasterBill
		{
			get { return searchMasterBill; }
			set
			{
				if (value != searchMasterBill)
				{
					CheckMaximumLength(SearchMasterBillInfo, value);
					SetNonPersistentPropertyValue(SearchMasterBillInfo, ref searchMasterBill, value);
				}
			}
		}

		public ZPropertyInfo SearchMasterBillInfo
		{
			get { return GetZPropertyInfo(nameof(SearchMasterBill)); }
		}

		#endregion

		#region SearchDepartureDate

		ZDateTime searchDepartureDate;
		public ZDateTime SearchDepartureDate
		{
			get { return searchDepartureDate; }
			set { SetNonPersistentPropertyValue(SearchDepartureDateInfo, ref searchDepartureDate, value); }
		}

		public ZPropertyInfo SearchDepartureDateInfo
		{
			get { return GetZPropertyInfo(nameof(SearchDepartureDate)); }
		}

		#endregion

		#region SearchPortOfLoading

		ZString searchPortOfLoading;
		[MaxLength(VoyageOrigin.Schema.JA_RL_NKPortOfLoadingMaxLength)]
		public ZString SearchPortOfLoading
		{
			get { return searchPortOfLoading; }
			set
			{
				if (value != searchPortOfLoading)
				{
					CheckMaximumLength(SearchPortOfLoadingInfo, value);
					SetNonPersistentPropertyValue(SearchPortOfLoadingInfo, ref searchPortOfLoading, value);
				}
			}
		}

		public ZPropertyInfo SearchPortOfLoadingInfo
		{
			get { return GetZPropertyInfo(nameof(SearchPortOfLoading)); }
		}

		#endregion

		#region SearchPortOfDischarge

		ZString searchPortOfDischarge;
		[MaxLength(VoyageDestination.Schema.JB_RL_NKPortOfDischargeMaxLength)]
		public ZString SearchPortOfDischarge
		{
			get { return searchPortOfDischarge; }
			set
			{
				if (value != searchPortOfDischarge)
				{
					CheckMaximumLength(SearchPortOfDischargeInfo, value);
					SetNonPersistentPropertyValue(SearchPortOfDischargeInfo, ref searchPortOfDischarge, value);
				}
			}
		}

		public ZPropertyInfo SearchPortOfDischargeInfo
		{
			get { return GetZPropertyInfo(nameof(SearchPortOfDischarge)); }
		}

		#endregion

		#region SearchFlightNumber

		ZString searchFlightNumber;
		[MaxLength(JobVoyage.Schema.JV_VoyageFlightMaxLength)]
		public ZString SearchFlightNumber
		{
			get { return searchFlightNumber; }
			set
			{
				if (value != searchFlightNumber)
				{
					CheckMaximumLength(SearchFlightNumberInfo, value);
					SetNonPersistentPropertyValue(SearchFlightNumberInfo, ref searchFlightNumber, value);
				}
			}
		}

		public ZPropertyInfo SearchFlightNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SearchFlightNumber)); }
		}

		#endregion

		#endregion

		#region LinkedConsolPk

		protected ForwardingConsol fLinkedConsol;
		public ForwardingConsol LinkedConsol
		{
			get { return fLinkedConsol; }
			set
			{
				if (value != fLinkedConsol)
				{
					SetLinkedConsolValue(value);
					LinkedConsolUniqueConsignRefInfo.RefreshBinding();
				}
			}
		}

		protected virtual void SetLinkedConsolValue(ForwardingConsol value)
		{
			fLinkedConsol = value;
		}

		void ReloadLinkedConsol(BusinessObjectFactory factory)
		{
			fLinkedConsol = factory.Load<ForwardingConsol>(LinkedConsol.PK);
			if (fLinkedConsol == null)
			{
				throw new InvalidOperationException("Cannot reload Linked Consol as it is not in database.");
			}
		}

		#endregion

		#region IsLinkedToConsol

		public ZBool IsLinkedToConsol
		{
			get { return (LinkedConsol != null); }
		}

		#endregion

		#endregion

		#region Collections

		#region NaturalMatchingConsols

		protected MainFormConsolCollection fNaturalMatchingConsols;
		public MainFormConsolCollection NaturalMatchingConsols
		{
			get
			{
				if (fNaturalMatchingConsols == null)
				{
					fNaturalMatchingConsols = new MainFormConsolCollection(Factory);
					LoadNaturalMatchingConsols();
				}

				return fNaturalMatchingConsols;
			}
		}

		void LoadNaturalMatchingConsols()
		{
			if (fNaturalMatchingConsols != null)
			{
				string sqlText = "";
				ZSqlParameterCollection @params = new ZSqlParameterCollection();

				if (MasterBillNum.IsEmpty)
				{
					string sailingFilter = @"
									JW_RL_NKLoadPort = @PortOfLoading
									AND JW_RL_NKDiscPort = @PortOfDischarge
									AND JW_VoyageFlight = @Flight
							";

					string transportFilter = @"
									JA_RL_NKPortOfLoading = @PortOfLoading
									AND JB_RL_NKPortOfDischarge = @PortOfDischarge
									AND JV_VoyageFlight = @Flight
							";

					if (DepartureDate.IsValid)
					{
						sailingFilter += "	AND (JW_ETD >= @DepartureDate AND JW_ETD < dateadd(day, 1, @DepartureDate))";
						transportFilter += "		AND (JA_E_DEP >= @DepartureDate AND JA_E_DEP < dateadd(day, 1, @DepartureDate))";
						@params.Add("@DepartureDate", DepartureDate, JobVoyOriginSchema.JA_E_DEP);
					}

					sqlText += @"
						JK_PK in
						(
							SELECT JW_ParentGUID
							FROM
								dbo.JobConsolTransport
								LEFT JOIN dbo.JobSailing ON JX_PK = JW_JX
								LEFT JOIN dbo.JobVoyOrigin ON JA_PK = JX_JA
								LEFT JOIN dbo.JobVoyDestination ON JB_PK = JX_JB
								LEFT JOIN dbo.JobVoyage ON JV_PK = JA_JV
							WHERE
								( " + sailingFilter + @" )
								OR
								( " + transportFilter + @" )
						";

					@params.Add("@PortOfLoading", PortOfLoading, JobVoyOriginSchema.JA_RL_NKPortOfLoading);
					@params.Add("@PortOfDischarge", PortOfDischarge, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge);
					@params.Add("@Flight", FlightNumber, JobVoyageSchema.JV_VoyageFlight);

					sqlText += @"
						) ";
				}
				else
				{
					sqlText += " JK_MasterBillNum = @MasterBillNum ";
					@params.Add("@MasterBillNum", MasterBillNum, JobConsolSchema.JK_MasterBillNum);
				}

				ZDBOnlyQuery consolFilter = new ZDBOnlyQuery(typeof(ForwardingConsol));
				consolFilter.AddFilterAndZSQLParameterCollection(sqlText, @params);
				ForwardingConsol[] freightConsols = (ForwardingConsol[])Factory.Load(typeof(ForwardingConsol), consolFilter);

				fNaturalMatchingConsols.RemoveAll();
				fNaturalMatchingConsols.AddRange(freightConsols);
			}
		}

		#endregion

		#region SearchMatchingConsols

		protected MainFormConsolCollection fSearchMatchingConsols;
		public MainFormConsolCollection SearchMatchingConsols
		{
			get
			{
				if (fSearchMatchingConsols == null)
				{
					fSearchMatchingConsols = new MainFormConsolCollection(Factory);
					LoadSearchMatchingConsols();
				}

				return fSearchMatchingConsols;
			}
		}

		void LoadSearchMatchingConsols()
		{
			if (fSearchMatchingConsols != null)
			{
				fSearchMatchingConsols.RemoveAll();

				ZSqlParameterCollection parameters = new ZSqlParameterCollection();

				// Append the MasterBill field
				StringBuilder masterBillSubquery = new StringBuilder();
				if (!SearchMasterBill.IsEmpty)
				{
					masterBillSubquery.Append(JobConsolSchema.JK_MasterBillNum.Name + " LIKE @MasterBillNum");
					parameters.Add("@MasterBillNum", "%" + SearchMasterBill + "%", JobConsolSchema.JK_MasterBillNum);
				}

				// Append search fields based on sailing data
				StringBuilder sailingSubquery = new StringBuilder();
				AppendSailingSubquery(sailingSubquery, parameters);

				if (parameters.Count > 0)
				{
					StringBuilder sqlQuery = new StringBuilder();

					if (masterBillSubquery.Length > 0)
					{
						sqlQuery.Append(string.Format("(SELECT {0} FROM {1} WHERE ", JobConsolSchema.PK.Name, JobConsolSchema.Constants.TableName));
						sqlQuery.Append(masterBillSubquery);
						sqlQuery.Append(")");
					}

					if (sailingSubquery.Length > 0)
					{
						if (sqlQuery.Length > 0)
						{
							sqlQuery.Append(" UNION ALL ");
						}

						sqlQuery.Append(string.Format("(SELECT {0} FROM {1} WHERE ", JobConsolSchema.PK.Name, JobConsolSchema.Constants.TableName));
						sqlQuery.Append(sailingSubquery);
						sqlQuery.Append(")");
					}

					sqlQuery.Insert(0, string.Format(" {0} in (", JobConsolSchema.PK.Name));
					sqlQuery.Append(") ");

					ZDBOnlyQuery consolFilter = new ZDBOnlyQuery(typeof(ForwardingConsol));
					consolFilter.AddFilterAndZSQLParameterCollection(sqlQuery.ToString(), parameters);
					ForwardingConsol[] freightConsols = (ForwardingConsol[])Factory.Load(typeof(ForwardingConsol), consolFilter);

					fSearchMatchingConsols.AddRange(freightConsols);
				}
			}
		}

		void AppendSailingSubquery(StringBuilder sqlQuery, ZSqlParameterCollection parameters)
		{
			StringBuilder sailingSubqueryFromClause = new StringBuilder();
			StringBuilder sailingSubqueryWhereClause = new StringBuilder();

			StringBuilder transportSubqueryWhereClause = new StringBuilder();

			if (SearchDepartureDate.IsValid || !SearchPortOfLoading.IsEmpty || !SearchFlightNumber.IsEmpty)
			{
				sailingSubqueryFromClause.Append(" INNER JOIN dbo.JobVoyOrigin ON JA_PK = JX_JA");

				if (SearchDepartureDate.IsValid)
				{
					AppendToWhereClause(sailingSubqueryWhereClause, "(JA_E_DEP >= @DepartureDate AND JA_E_DEP < dateadd(day, 1, @DepartureDate))");
					AppendToWhereClause(transportSubqueryWhereClause, "(JW_ETD >= @DepartureDate AND JW_ETD < dateadd(day, 1, @DepartureDate))");
					parameters.Add("@DepartureDate", SearchDepartureDate.Date, JobVoyOriginSchema.JA_E_DEP);
				}

				if (!SearchPortOfLoading.IsEmpty)
				{
					AppendToWhereClause(sailingSubqueryWhereClause, " JA_RL_NKPortOfLoading = @PortOfLoading");
					AppendToWhereClause(transportSubqueryWhereClause, " JW_RL_NKLoadPort = @PortOfLoading");
					parameters.Add("@PortOfLoading", SearchPortOfLoading, JobVoyOriginSchema.JA_RL_NKPortOfLoading);
				}

				if (!SearchFlightNumber.IsEmpty)
				{
					sailingSubqueryFromClause.Append(" INNER JOIN dbo.JobVoyage ON JV_PK = JA_JV");
					AppendToWhereClause(sailingSubqueryWhereClause, " JV_VoyageFlight LIKE @Flight");
					AppendToWhereClause(transportSubqueryWhereClause, " JW_VoyageFlight LIKE @Flight");
					parameters.Add("@Flight", "%" + SearchFlightNumber + "%", JobVoyageSchema.JV_VoyageFlight);
				}
			}

			if (!SearchPortOfDischarge.IsEmpty)
			{
				sailingSubqueryFromClause.Append(" INNER JOIN dbo.JobVoyDestination ON JB_PK = JX_JB");
				AppendToWhereClause(sailingSubqueryWhereClause, " JB_RL_NKPortOfDischarge = @PortOfDischarge");
				AppendToWhereClause(transportSubqueryWhereClause, " JW_RL_NKDiscPort = @PortOfDischarge");
				parameters.Add("@PortOfDischarge", SearchPortOfDischarge, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge);
			}

			if (sailingSubqueryFromClause.Length > 0)
			{
				AppendToWhereClause(sailingSubqueryWhereClause, " JW_IsLinked = 1");
				AppendToWhereClause(transportSubqueryWhereClause, " JW_IsLinked = 0");

				string sailingFilter = @"
					JK_PK in
					(
						SELECT JW_ParentGUID
						FROM dbo.JobConsolTransport
						JOIN dbo.JobSailing ON JW_JX = JX_PK
					";

				string transportFilter = @"
                        UNION ALL
                        SELECT JW_ParentGUID
						FROM dbo.JobConsolTransport
					";

				sqlQuery.Append(sailingFilter);
				sqlQuery.Append(sailingSubqueryFromClause);
				sqlQuery.Append(sailingSubqueryWhereClause);

				sqlQuery.Append(transportFilter);
				sqlQuery.Append(transportSubqueryWhereClause);

				sqlQuery.Append(")");
			}
		}

		void AppendToWhereClause(StringBuilder whereClause, string textToAppend)
		{
			if (whereClause.Length == 0)
			{
				whereClause.Append(" WHERE ");
			}
			else
			{
				whereClause.Append(" AND ");
			}

			whereClause.Append(textToAppend);
		}

		#endregion

		#region MatchingConsols = NaturalMatchingConsols + SearchMatchingConsols

		protected MainFormConsolCollection fMatchingConsols;
		public MainFormConsolCollection MatchingConsols
		{
			get
			{
				if (fMatchingConsols == null)
				{
					fMatchingConsols = new MainFormConsolCollection(Factory);
					fMatchingConsols.SetReadOnlyIncludingChildren(true);
					LoadMatchingConsols();
				}

				return fMatchingConsols;
			}
		}

		protected void LoadMatchingConsols()
		{
			if (fMatchingConsols != null)
			{
				fMatchingConsols.RemoveAll();
				fMatchingConsols.AddRange(NaturalMatchingConsols);
				fMatchingConsols.AddRange(SearchMatchingConsols);
			}
		}

		#endregion

		#endregion

		public void SearchEnterpriseConsols()
		{
			LoadSearchMatchingConsols();
			LoadMatchingConsols();
		}

		internal void LinkMawbShipmentsToConsolCreatingNonExisting(INotifications notify, bool createDeclaration)
		{
			LinkMawbShipmentsToConsolCreatingNonExisting(notify, createDeclaration, 0, 1);
		}

		/// <summary>
		/// Link shipments on this MAWB segment to its LinkedConsol.
		/// Create Shipments and Shipment Notes if they don't exist yet.
		/// </summary>
		public void LinkMawbShipmentsToConsolCreatingNonExisting(INotifications notify, bool createDeclaration, int noOfProcessedRecords, int totalNoOfRecords)
		{
			if (LinkedConsol != null)
			{
				BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
				ReloadLinkedConsol(factoryProvider.Current);

				ULDNo = "";
				PackContainer = ZGuid.Empty;
				int numberOfShipments = fQuantumSegment.Shipments.Length;

				// Find (or create new) Enterprise Shipment for each Shipment Record 
				// and link it to the MAWB Consol

				factoryProvider.Current.SuspendValidation();

				Dictionary<ZGuid, int> shipmentListToBeSaved = new Dictionary<ZGuid, int>();
				int percentComplete = (noOfProcessedRecords * 100) / totalNoOfRecords;

				try
				{
					for (int i = 0; i < numberOfShipments; i++)
					{
						QuantumShipmentRecord shipmentRecord = fQuantumSegment.Shipments[i];
						ZString processingHousebill = shipmentRecord.HouseBill;
						percentComplete = (noOfProcessedRecords + i) * 100 / totalNoOfRecords;
						FireOnProgress(percentComplete, "Processing HouseBill: " + processingHousebill);

						if (ULDRequired)
						{
							GetULD(shipmentRecord.MBagNo);
						}

						ForwardingShipment shipment = CreateShipment(shipmentRecord, notify, percentComplete, createDeclaration, factoryProvider.Current);
						if (shipment != null)
						{
							shipmentListToBeSaved[shipment.PK] = i;
							shipment = null;
						}

						if (i != 0 && i % 50 == 0)
						{
							FireOnProgress(percentComplete, "Saving Current 50 Shipments to Database ...");

							UpdateAndLinkShipmentToConsol(shipmentListToBeSaved, percentComplete, factoryProvider.Current);
							factoryProvider.SaveCurrentAndCreateNew();
							ReloadLinkedConsol(factoryProvider.Current);

							shipmentListToBeSaved.Clear();
							factoryProvider.Current.SuspendValidation();
						}
					}

					UpdateAndLinkShipmentToConsol(shipmentListToBeSaved, percentComplete, factoryProvider.Current);
					RemoveMawbShipmentLinksNotInInterfaceFileIfConsolNotProcessedBefore();
					factoryProvider.SaveCurrentAndCreateNew();
					ReloadLinkedConsol(factoryProvider.Current);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notify.Notify(new ErrorNotification(ErrorType.Error,
						"Error(s) occurred when importing data for Consol: " + LinkedConsol.JK_UniqueConsignRef + "- " + ex.Message));
				}
			}
		}

		void UpdateAndLinkShipmentToConsol(Dictionary<ZGuid, int> shipmentListToBeSaved, int percentComplete, BusinessObjectFactory factory)
		{
			foreach (KeyValuePair<ZGuid, int> current in shipmentListToBeSaved)
			{
				ForwardingShipment shipment = factory.Load<ForwardingShipment>(current.Key);
				AddGoodsDescriptionNote(fQuantumSegment.ShipmentsNotes[current.Value], shipment);
				UpdateShipmentAndLinkToConsol(shipment, fQuantumSegment.Shipments[current.Value].BranchCode, percentComplete);
			}
		}

		public event TNTProgressEventHandler OnProgress;

		#region Implementation

		protected virtual void AddGoodsDescriptionNote(QuantumShipmentNotesRecord noteRecord, ForwardingShipment shipment)
		{
			noteRecord.CreateShipmentNoteIfNotExists(Factory, shipment);
		}

		internal QuantumSegment fQuantumSegment;
		protected virtual void UpdateShipmentAndLinkToConsol(ForwardingShipment shipment, ZString branchCode, int percentComplete)
		{
			if (shipment != null)
			{
				FireOnProgress(percentComplete, string.Format("Linking HAWB '{0}' to Consol '{1}'", shipment.JS_HouseBill, LinkedConsolUniqueConsignRef));
				if (!string.IsNullOrEmpty(ULDNo) && ULDNo != "NA")
				{
					if (shipment.OuterPackLines.Count > 0)
					{
						JobContainerPackPivot pivot = shipment.Factory.New<JobContainerPackPivot>();
						pivot.J6_JC = PackContainer;
						pivot.J6_JL = shipment.OuterPackLines[0].PK;
					}
				}

				UpdateShipmentDetailsFromConsol(shipment);

				if (!LinkedConsol.Shipments.Contains(shipment.PK))
				{
					if (shipment.CustomsEntryNumberType.StartsWith("EX"))
					{
						shipment.CusEntryNumbers[0][CusEntryNumSchema.CE_EntryLineReference.Name] = "";
					}

					LinkedConsol.Shipments.Add(shipment);
				}
				CreateAndSyncroniseJobDeclaration(shipment, branchCode);
				HousebillsInInterfaceFileList.Instance.Add(LinkedConsolUniqueConsignRef, shipment.PK);
			}
		}

		protected void UpdateShipmentDetailsFromConsol(ForwardingShipment shipment)
		{
			shipment.JS_E_DEP = LinkedConsol.JK_JX_JA_E_DEP;
			shipment.JS_E_ARV = LinkedConsol.JK_JX_JB_E_ARV;

			if (shipment.Declarations.Length > 0)
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)shipment.Declarations[0];
				declaration.JE_DateAtOrigin = LinkedConsol.JK_JX_JA_E_DEP;
				declaration.JE_DateOfArrival = LinkedConsol.JK_JX_JB_E_ARV;
				declaration.JE_MasterBill = LinkedConsol.JK_MasterBillNum;
				declaration.JE_VoyageFlightNo = LinkedConsol.JK_JX_JV_VoyageFlight;
			}
		}

		protected virtual void RemoveMawbShipmentLinksNotInInterfaceFileIfConsolNotProcessedBefore()
		{
			if (LinkedConsol != null && !HasMawbBeenProcessedBefore(LinkedConsolUniqueConsignRef))
			{
				ArrayList housebillsToDetach = new ArrayList();

				foreach (CommonShipment shipment in LinkedConsol.Shipments)
				{
					if (!HousebillsInInterfaceFileList.Instance.Contains(LinkedConsolUniqueConsignRef, shipment.PK))
					{
						housebillsToDetach.Add(shipment.PK);
					}
				}

				foreach (ZGuid shipmentPK in housebillsToDetach)
				{
					LinkedConsol.Shipments.Remove(shipmentPK);
				}
			}
		}

		protected bool HasMawbBeenProcessedBefore(ZString consolRef)
		{
			return MawbInterfaceDetailSaver.IsMawbInterfaceDetailStored(consolRef);
		}

		protected void GetULD(string mBagNo)
		{
			string consol = LinkedConsolUniqueConsignRef;
			string mawb = fQuantumSegment.Consol.MasterBill;
			if (mBagNo != SavedMBagNo)
			{
				SavedMBagNo = mBagNo;
				ULDNo = null;
				while (ULDNo == null)
				{
					using (var ex2ContainerForm = new Exit2ULDForm(consol, mawb, mBagNo))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(ex2ContainerForm) == DialogResult.OK)
						{
							ULDNo = ex2ContainerForm.ULDNo;
							if (ULDNo != null && ULDNo != "NA")
							{
								CreateConsolContainer(LinkedConsol, ULDNo);
							}
						}
					}
				}
			}
		}

		protected void CreateConsolContainer(ForwardingConsol enterpriseConsol, string uLDNo)
		{
			if (enterpriseConsol.Containers.HasContainer(uLDNo))
			{
				fContainers = new CommonContainerCollection(enterpriseConsol, Factory);
				fContainers.Load(new ZQuery(new ZQuery(JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.Equal, uLDNo),
					JoinCondition.And, new ZQuery(JobContainerSchema.JC_JK, SQLComparisonOperator.Equal, enterpriseConsol.PK)));
				if (fContainers.Count == 0)
				{
					PackContainer = ZGuid.Empty;
				}
				else
				{
					PackContainer = fContainers[0].PK;
				}
			}
			else
			{
				CommonContainer consolContainer = enterpriseConsol.Containers.AddNew();
				consolContainer.JC_ContainerNum = uLDNo;
				PackContainer = consolContainer.PK;
			}
		}

		ForwardingShipment CreateShipment(QuantumShipmentRecord shipmentRecord, INotifications notify, int percentageComplete, bool createDeclaration, BusinessObjectFactory factory)
		{
			return shipmentRecord.FindFirstMatchingShipment(LinkedConsol) ?? shipmentRecord.CreateAndSaveShipment(factory, notify, percentageComplete, createDeclaration);
		}

		protected void FireOnProgress(int percentComplete, ZString message)
		{
			if (OnProgress != null)
			{
				OnProgress(this, new TNTProgressEventArgs(percentComplete, message));
			}
		}

		protected virtual void CreateAndSyncroniseJobDeclaration(ForwardingShipment shipment, ZString branchCode)
		{
			// Do nothing for AU
		}

		protected string ULDNo;
		protected string SavedMBagNo;
		protected ZGuid PackContainer;
		CommonContainerCollection fContainers;

		#endregion
	}
}
