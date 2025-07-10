using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema("UniversalEvent.xsd"), RootElement("UniversalEvent")]
	public partial class Event : TopLevelDataObject
	{
		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		[Mandatory]

		public UXmlDateTime? EventTime { get; set; }
		[Mandatory, MaxLength(3), CodeMap(Constants.OrgPatternMatchOverrideRelationships.EventCode)]
		public ZString? EventType { get; set; }
		[MaxLength(1024)]
		public ZString? EventReference { get; set; }
		public ZDateTimeOffset? CreatedTime { get; set; }
		public ZBool? IsEstimate { get; set; }
		public ZBool? IsCancelled { get; set; }
		public EventParameters EventParameters { get; set; }

		public List<Context> ContextCollection { get; set; }
		public List<AdditionalContext> AdditionalContextCollection { get; set; }
		public List<AdditionalFieldToUpdate> AdditionalFieldsToUpdateCollection { get; set; }
		public List<AttachedDocument> AttachedDocumentCollection { get; set; }

		#region IDisposable Support

		bool disposed;

		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					if (AttachedDocumentCollection != null)
					{
						foreach (var attachedDocument in AttachedDocumentCollection)
						{
							attachedDocument?.Dispose();
						}
					}
				}

				disposed = true;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
