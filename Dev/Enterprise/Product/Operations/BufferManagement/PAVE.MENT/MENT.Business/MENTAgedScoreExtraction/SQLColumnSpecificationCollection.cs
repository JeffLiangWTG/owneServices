using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public abstract class SQLColumnSpecificationCollection : NonPersistentBusinessObjectCollection<SQLColumnSpecification>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected SQLColumnSpecificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			InitialiseCollectionForDeserialisation();
		}

		protected override void InitialiseCollectionForDeserialisation()
		{
			Build();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SQLColumnSpecification(Factory);
		}

		#region Build

		public void Build()
		{
			var columnsToAdd = ColumnsToAddOnBuild().ToArray();
			if (Count != columnsToAdd.Length)
			{
				foreach (var column in columnsToAdd)
				{
					Add(column);
					column.HasChanges = false;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element name")]
		protected override NonPersistentBusinessObject GetItemToDeserialise(XElement element)
		{
			var code = element.Descendants("Code").FirstOrDefault();
			return code != null ? this[code.Value] : null;
		}

		public SQLColumnSpecification this[string code]
		{
			get { return this.Cast<SQLColumnSpecification>().FirstOrDefault(i => i.Code == code); }
		}

		protected abstract Collection<SQLColumnSpecification> ColumnsToAddOnBuild();

		#endregion
	}
}
