using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;

namespace Enterprise.DbUpgrader.Script
{
	public class DbRoutineScriptCollection : IEnumerable<IDbScript>, IEnumerable
	{
		public void Add(IDbScript element)
		{
			if (scriptDictionary.ContainsKey(element.SchemaName + "." + element.Name))
			{
				throw new ArgumentException(String.Format("Element [{0}] has already been added to the collection.", element.SchemaName + "." + element.Name));
			}

			AddToInternalLists(element.SchemaName + "." + element.Name, element);
		}

		public void AddClientSpecific(IDbScript element)
		{
			int existingIndex;
			if (scriptDictionary.TryGetValue(element.SchemaName + "." + element.Name, out existingIndex))
			{
				scriptList[existingIndex] = element;
			}
			else
			{
				AddToInternalLists(element.SchemaName + "." + element.Name, element);
			}
		}

		public IEnumerator<IDbScript> GetEnumerator()
		{
			return scriptList.GetEnumerator();
		}

		public bool HasElements
		{
			get { return scriptList.Count > 0; }
		}

		readonly List<IDbScript> scriptList = new List<IDbScript>();
		readonly Dictionary<string, int> scriptDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		void AddToInternalLists(string key, IDbScript element)
		{
			scriptDictionary.Add(key, scriptList.Count);
			scriptList.Add(element);
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}

	class DbScript : IDbScript
	{
		public DbScript(string scriptName, string scriptText, string scriptObjectType)
			: this(Db.SqlDbOwnerSchema, scriptName, scriptText, scriptObjectType) { }

		public DbScript(string schemaName, string scriptName, string scriptText, string scriptObjectType)
		{
			this.SchemaName = schemaName;
			this.Name = scriptName;
			this.Text = scriptText;
			this.ObjectType = scriptObjectType;
		}

		public string SchemaName { get; private set; }
		public string Name { get; private set; }
		public string Text { get; private set; }
		public string ObjectType { get; private set; }
	}

	class IndexedViewDbScript : DbScript, IIndexedViewDbScript
	{
		public IndexedViewDbScript(string scriptName, string scriptText, string scriptObjectType, string indexCreateScript)
			: base(scriptName, scriptText, scriptObjectType)
		{
			this.IndexCreateScript = indexCreateScript;
		}

		public string IndexCreateScript { get; private set; }
	}
}
