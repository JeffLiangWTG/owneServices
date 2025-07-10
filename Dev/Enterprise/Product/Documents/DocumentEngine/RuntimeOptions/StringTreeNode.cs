using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class StringTreeNode : IJsonSerializable
	{
		public StringTreeNode()
		{
		}

		#region Constructor For IJsonSerializable

		internal StringTreeNode(StringTreeNodeJsonData data)
		{
			Value = data.Value;
			CellReference = new CellReference(data.CellReference);
			Children.AddRange(data.Children.Select(c => new StringTreeNode(c)).ToArray());
		}

		#endregion

		public static StringTreeNode Deserialise(ZBlob blob)
		{
			return Deserialise_JsonFormat(blob);
		}

		public static StringTreeNode Deserialise_JsonFormat(ZBlob blob)
		{
			StringTreeNode newNode;
			if (blob == null || blob == ZBlob.Empty)
			{
				newNode = new StringTreeNode();
			}
			else
			{
				using (var stream = new MemoryStream(blob))
				using (var reader = new StreamReader(stream))
				{
					var json = reader.ReadToEnd();
					newNode = JsonConverterHelper.Deserialize<StringTreeNode>(json);
				}
			}

			return newNode;
		}

		public static ZBlob Serialise(StringTreeNode nodeToSerialise)
		{
			var json = JsonConverterHelper.Serialize(nodeToSerialise);
			return new ZBlob(Encoding.UTF8.GetBytes(json));
		}

		public string Value = "";
		public CellReference CellReference = CellReference.UnKnown;
		public readonly StringTreeNodeCollection Children = new StringTreeNodeCollection();

		public override string ToString()
		{
			return "<" + Value + "> " + Children.ToString();
		}

		public StringTreeNodeCollection LastCollectionAtLevel(int level)
		{
			var result = Children;
			for (var i = 0; i < level; i++)
			{
				if (result.Count > 0)
				{
					result = result.Last().Children;
				}
				else
				{
					string message;
					if (Children.Count == 0)
					{
						message = Res.GetString("FE510B9B-10C3-4BD4-B2A6-DC2527490BC1", "The filter at row 1 does not have a Display Name. Please enter a value for it.");
					}
					else
					{
						var currentFilterName = Children.Last().Value;
						message = Res.GetString("DFBACD84-DE2C-45B8-B623-B6B5714AC716", "The filter '{0}' is not properly configured. Please make sure you have provided a type and other suitable options for that filter.", currentFilterName);
					}

					throw new TemplateDefinitionException(message, CellReference);
				}
			}

			return result;
		}

		public bool ChildExists(string value)
		{
			foreach (StringTreeNode n in Children)
			{
				if (value.Equals(n.Value, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public StringTreeNode FindChild(string value)
		{
			StringTreeNode result = null;
			foreach (StringTreeNode n in Children)
			{
				if (value.Equals(n.Value, StringComparison.OrdinalIgnoreCase))
				{
					if (result == null)
					{
						result = n;
					}
					else
					{
						throw new TemplateDefinitionException(
							string.Format(@"There is more than one ""{0}"" under the ""{1}"" block", value, Value),
							n.CellReference);
					}
				}
			}

			if (result != null)
			{
				return result;
			}
			else
			{
				throw new TemplateDefinitionException(string.Format(@"There is no ""{0}"" under the ""{1}"" block", value, Value), CellReference);
			}
		}

		public bool TryFindSingleChild(string value, out StringTreeNode stringTreeNode)
		{
			stringTreeNode = null;
			foreach (StringTreeNode childNode in Children)
			{
				if (value.Equals(childNode.Value, StringComparison.OrdinalIgnoreCase))
				{
					if (stringTreeNode == null)
					{
						stringTreeNode = childNode;
					}
					else
					{
						stringTreeNode = null;
						break;
					}
				}
			}

			return stringTreeNode != null;
		}

		public StringTreeNode Child()
		{
			if (Children.Count == 1)
			{
				return Children[0];
			}
			else
			{
				throw new TemplateDefinitionException(string.Format(@"""{0}"" should have one and only one item under it", Value), CellReference);
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new StringTreeNodeJsonData
			{
				Value = Value,
				CellReference = (CellReferenceJsonData)CellReference.GetJsonData(),
				Children = Children.Cast<StringTreeNode>().Select(n => (StringTreeNodeJsonData)n.GetJsonData()).ToList()
			};

		#endregion
	}
}
