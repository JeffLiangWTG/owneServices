using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NCTSPrettierTableBase : INCTSPrettierTable
	{
		#region Implementation of INCTSPrettierTable

		public abstract ZString Caption { get; }

		public virtual ZString Border => "1";

		public virtual ZString Width => "100%";

		public abstract IReadOnlyCollection<(ZString Caption, ZString Attributes)> Columns { get; }

		public abstract IReadOnlyCollection<(ZString Key, ZString Value)> AdditionalInfo { get; }

		#endregion

		protected abstract IEnumerable<object[]> Rows { get; }

		public override bool Equals(object obj) => obj is NCTSPrettierTableBase other && Equals(other);

		public bool Equals(NCTSPrettierTableBase other)
			=> ReferenceEquals(this, other) ||
				other is not null && string.Equals(ToString(), other.ToString(), StringComparison.Ordinal);

		public override int GetHashCode() => (Caption, Columns, AdditionalInfo, Rows, Border, Width).GetHashCode();

		public override string ToString()
		{
			using var rowEnumerator = Rows.GetEnumerator();
			if (!rowEnumerator.MoveNext())
			{
				return string.Empty;
			}

			var tableCreator = HtmlHelper.GetHtmlTableCreator(border: Border, width: Width);

			tableCreator.WriteRowWithFormatting(Columns.Select(x => new CellWithFormatting(x.Caption, GetCellAttributes(x.Attributes))).ToArray());
			foreach (var row in Rows)
			{
				tableCreator.WriteRow(row.Select(GetCellOutput).ToArray());
			}

			var result = new StringBuilder();
			result.Append(HtmlHelper.ToH3IfNotEmpty(Caption));
			result.Append(HtmlHelper.ToKeyValuePairSection(AdditionalInfo.Select(x => (x.Key.ToString(), x.Value.ToString()))));
			result.Append(tableCreator.ToHtml());
			return result.ToString();

			NameValueCollection GetCellAttributes(string argAttributes)
			{
				var attributesCollection = new NameValueCollection();
				foreach (var attributeDescriptor in argAttributes.Split(';'))
				{
					var keyValuePair = attributeDescriptor.Split('=');
					if (keyValuePair.Length != 2)
					{
						continue;
					}
					attributesCollection.Add(keyValuePair[0], keyValuePair[1]);
				}

				return attributesCollection;
			}

			object GetCellOutput(object cellValue)
				=> cellValue switch
				{
					null => string.Empty,
					INCTSPrettierAdditionalBlock nestedBlock => HtmlHelper.ToPIfNotEmpty(nestedBlock.ToString()),
					_ => cellValue.ToString()
				};
		}
	}
}
