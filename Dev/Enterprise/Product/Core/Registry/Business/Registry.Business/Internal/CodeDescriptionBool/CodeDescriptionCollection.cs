using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class CodeDescriptionCollection<T, P> : RegistryBusinessObjectCollection, ICodeDescriptionPairList, IRegistryCollectionToTVP
		where T : CodeDescription<P>, ICodeDescription<P>, new()
		where P : IZType
	{
		protected CodeDescriptionCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory, ReadOnlyCodeDescriptionPairList list, P defaultValueForNewChild, int codeMaxLength)
			: base(fallbackLevel, factory, list, codeMaxLength)
		{
			DefaultValueForNewChild = defaultValueForNewChild;
		}

		public new T this[int i]
		{
			get { return (T)base[i]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		public void Set(string code, P value)
		{
			var pair = (T)FindByCode(code) ?? throw new ArgumentException(FormattableString.Invariant($"Code [{code}] was not in the collection"));
			pair.Value = value;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new T();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((T)child).Value = DefaultValueForNewChild;
		}

		internal void InitializeFromDefaultValue(CodeDescriptionCollection<T, P> defaultValue)
		{
			if (!initialized)
			{
				CodeMaxLength = defaultValue.CodeMaxLength;
				DefaultValueForNewChild = defaultValue.DefaultValueForNewChild;
				initialized = true;
			}
		}
		bool initialized;

		public P DefaultValueForNewChild { get; protected set; }

		public string GetDescriptionFromCode(string code)
		{
			var element = (T)FindByCode(code);
			return element?.Description ?? ZString.Empty;
		}

		public bool ContainsCode(object code)
		{
			return base.ContainsCode(code.ToString());
		}

		#region IRegistryCollectionToTVP Members

		public string TVPType => "TVP_CodeDescriptionMapping";

		public DataTable CreateDataTable()
		{
			var table = new DataTable();

			table.Locale = CultureInfo.InvariantCulture;
			table.Columns.Add((NoResString)"Code", typeof(string));
			table.Columns.Add((NoResString)"Description", typeof(string));

			var collection = this.OfType<T>().ToList();
			foreach (var item in collection)
			{
				table.Rows.Add((string)item.Code, (string)item.Description);
			}

			return table;
		}

		#endregion
	}
}
