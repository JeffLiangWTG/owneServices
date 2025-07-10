using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class DataWritingManager : IDataWritingManager
	{
		public DataWritingManager(IUniversalActionInfo action, IDataWritingInformationCollector informationCollector = null, IUniversalXmlSchema schema = null, IDataObjectWriterStrategy writerStrategy = null)
		{
			this.Action = Argument.NotNull(action, "action");
			this.informationCollector = informationCollector;

			Schema = schema ?? SchemaVersionManager.Current;
			WriterStrategy = writerStrategy ?? DefaultDataObjectWriterStrategy.Instance;
		}

		readonly IDataWritingInformationCollector informationCollector;

		public IUniversalActionInfo Action { get; private set; }

		public IUniversalXmlSchema Schema { get; private set; }

		public bool OverrideSendCostingData { get; set; }

		public bool ShouldPopulateInternalMilestones { get; set; }

		public DataContextType? FilteredDataContextType { get; set; }

		public IDataObjectWriterStrategy WriterStrategy { get; }

		#region Duplicate PK Checking

		HashSet<ZGuid> pksUsed = new HashSet<ZGuid>();

		public bool PKAlreadyExported(ZGuid pk)
		{
			return pksUsed.Contains(pk);
		}

		public void AddPK(ZGuid pk)
		{
			pksUsed.Add(pk);
		}

		public void NotifyExported(IDataObject dataObject, BusinessObject businessObject)
		{
			if (informationCollector != null)
			{
				informationCollector.NotifyExported(dataObject, businessObject);
			}
		}

		public IDisposable UseNewListForDuplicatePKCheck()
		{
			return new DuplicatePKCheckStacker(this);
		}

		class DuplicatePKCheckStacker : Disposable
		{
			internal DuplicatePKCheckStacker(DataWritingManager writeManager)
			{
				this.pksUsed = writeManager.pksUsed;
				this.writeManager = writeManager;
				writeManager.pksUsed = new HashSet<ZGuid>();
			}
			readonly HashSet<ZGuid> pksUsed;
			readonly DataWritingManager writeManager;

			protected override void Dispose(bool isDisposing)
			{
				writeManager.pksUsed = pksUsed;
			}
		}

		#endregion

		public IEDIMessageContentFilterManager ContentFilterManager
		{
			get
			{
				if (contentFilterManager == null)
				{
					contentFilterManager = new EDIMessageContentFilterManager(Action.FactoryForProcessing, Action.PurposeCode);
				}
				return contentFilterManager;
			}
		}

		IEDIMessageContentFilterManager contentFilterManager;

		#region Internal Publish

		public bool IsPublishingInternally { get; private set; }

		public IDisposable SetIsPublishingInternally()
		{
			return new InternalPublishSetter(this);
		}

		class InternalPublishSetter : IDisposable
		{
			public InternalPublishSetter(DataWritingManager writeManager)
			{
				this.writeManager = writeManager;
				this.writeManager.IsPublishingInternally = true;
			}

			readonly DataWritingManager writeManager;

			public void Dispose()
			{
				this.writeManager.IsPublishingInternally = false;
			}
		}

		#endregion
	}
}
