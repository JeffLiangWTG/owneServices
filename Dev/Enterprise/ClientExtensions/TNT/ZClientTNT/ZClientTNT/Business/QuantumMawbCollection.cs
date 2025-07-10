using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT
{
	public class QuantumMawbCollection : NonPersistentBusinessObjectCollection<QuantumMawb>	{
		public QuantumMawbCollection(BusinessObjectFactory factory, string exit2FileFullName)
			: base(factory)
		{
			fExit2FileWrapper = QuantumFile.GetFile(GetNewRecordFactory(), exit2FileFullName, new NotificationBuffer(null));
		}

		#region Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("This collection doesn't allow new objects to be created");
		}

		#endregion

		/// <summary>
		/// Add Elements (MAWBs) to the collection from Exit2 file Segment 01
		/// </summary>
		public void LoadFromFile()
		{
			if (fExit2FileWrapper != null && fExit2FileWrapper.QuantumSegments != null)
			{
				foreach (QuantumSegment segment in fExit2FileWrapper.QuantumSegments)
				{
					AddQuantumMawb(Factory, segment);
				}
			}
		}

		readonly QuantumFile fExit2FileWrapper;

		protected virtual QuantumRecordFactory GetNewRecordFactory()
		{
			return new Exit2QuantumRecordFactory();
		}

		protected virtual void AddQuantumMawb(BusinessObjectFactory factory, QuantumSegment segment)
		{
			Add(new QuantumMawb(factory, segment));
		}
	}
}
