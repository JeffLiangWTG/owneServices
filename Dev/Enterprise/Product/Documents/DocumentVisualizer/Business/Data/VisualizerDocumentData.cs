using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DataTransformation;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	[DebuggerDisplay("{JDD_Name} ({JDD_ParentTableCode}|{JDD_ParentID})")]
	[UniversalDataContext(DataContextType.DocumentData)]
	public sealed class VisualizerDocumentData : AutoJobDocumentData, IProcessHandlingInfoProvider, IVisualizerDocumentData, IJobNumber
	{
		public VisualizerDocumentData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(JDD_OverriddenData), ConcurrencyPolicy.Ignore);
			RegisterEditableChildObject(Logs);
		}

		#region Parent

		object IVisualizerDocumentData.Parent
		{
			get => Parent;
			set
			{
				if (value is BusinessObject bizObj)
				{
					Parent = bizObj;
				}
			}
		}

		public BusinessObject Parent
		{
			get => parent ?? FindParent();
			set
			{
				if (parent == value)
				{
					return;
				}

				parent = value;

				if (parent != null)
				{
					JDD_ParentID = parent.PK;
					JDD_ParentTableCode = parent.TablePrefix;
				}
				else
				{
					JDD_ParentID = ZGuid.Empty;
					JDD_ParentTableCode = ZString.Empty;
				}
			}
		}

		BusinessObject parent;

		BusinessObject FindParent()
		{
			if (parent != null)
			{
				return parent;
			}

			if (JDD_ParentID.IsValid)
			{
				var loadedParent = Factory.GetBizOsForPK(JDD_ParentID.ToGuid())
					.FirstOrDefault(bizObj => bizObj.TablePrefix == JDD_ParentTableCode);

				return loadedParent ?? Factory.Load(JDD_ParentTableCode, JDD_ParentID);
			}

			return null;
		}

		#endregion

		#region Name

		public string Name
		{
			get => JDD_Name;
			set => JDD_Name = value;
		}

		#endregion

		#region JDD_ParentID

		public override ZGuid JDD_ParentID
		{
			get { return base.JDD_ParentID; }
			set
			{
				if (value != base.JDD_ParentID)
				{
					base.JDD_ParentID = value;

					if (!value.IsValid || parent != null && parent.PK != value)
					{
						parent = null;
					}
				}
			}
		}

		#endregion

		#region JDD_ParentTableCode

		public override ZString JDD_ParentTableCode
		{
			get { return base.JDD_ParentTableCode; }
			set
			{
				if (value != base.JDD_ParentTableCode)
				{
					base.JDD_ParentTableCode = value;

					if (!value.IsValid || parent != null && parent.TablePrefix != value)
					{
						parent = null;
					}
				}
			}
		}

		#endregion

		#region Xml

		public XDocument ReadXml()
		{
			if (JDD_OverriddenData.IsEmpty)
			{
				return null;
			}

			return XDocument.Parse(JDD_OverriddenData);
		}

		public void WriteXml(XDocument document)
		{
			JDD_OverriddenData = document != null && document.Root != null
				? document.AddDataVersion().ToString(SaveOptions.DisableFormatting)
				: string.Empty;
		}

		#endregion

		#region Save

		public void Save()
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, null);
		}

		#endregion

		#region SystemLastEditUser

		public string SystemLastEditUser
		{
			get { return JDD_SystemLastEditUser; }
		}

		#endregion

		#region IProcessHandlingInfoProvider

		public ProcessHandlingInfo ProcessHandlingInfo
		{
			get { return new VisualizerDocumentDataProcessHandlingInfo(this); }
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return string.Empty; }
		}

		#endregion

		#region ProcessLogCore

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			var processorDict = ObjectFactory.Get<Hashtable>("VisualizerDocumentDataIncomingEventProcessors");
			var handle = (ObjectHandle)processorDict[JDD_ParentTableCode.ToString()];

			if (handle?.GetObject() is IVisualizerDocumentDataIncomingEventProcessor processor)
			{
				processor.Process(this, log);
			}
		}

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("8460b502-0e9a-4b08-958f-62fb9412d7c5", "'{0}' data", JDD_Name); }
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				ObjectFactory.Get<ILicenceConsumptionLogCreator>().CreateLog(Env.Licence.FormBuilder);
			}
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new VisualizerDocumentDataUniqueIndexFailureHandler()); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class VisualizerDocumentDataUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			IEnumerable<string> IUniqueIndexFailureHandler.HandledUniqueIndexNames
			{
				get { yield return JobDocumentDataSchema.Constants.Indexes.NR_UC__JDD_ParentTableCode_JDD_ParentID_JDD_Name; }
			}

			void IUniqueIndexFailureHandler.NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var errorMsg = Res.GetString("4c03f5a2-e1a0-4ce3-a2c6-211dd1f09156", "Another user has saved changes to this Form while you were working on it. Please close and re-open the Form for the latest changes.");
				var caption = Res.GetString("cafea64e-14c5-4013-b254-3674d082827c", "Error");
				notifier.ReportError(errorMsg, caption);
			}
		}

		#endregion

		#endregion
	}
}
