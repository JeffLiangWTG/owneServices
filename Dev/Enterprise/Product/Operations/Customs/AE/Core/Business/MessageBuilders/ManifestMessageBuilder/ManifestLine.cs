using System.Text;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public abstract class ManifestLine : FlatFileDataRow
{
	public ManifestLine(FlatFileDataRow row) : base(row) { }
	public ManifestLine(int fieldCount)
		: base(fieldCount)
	{
		SetPreambleData();
	}

	public override string ToString()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine(new ManifestLineFormat().ConvertToLine(this));

		foreach (ManifestLine line in Children)
		{
			builder.Append(line.ToString());
		}
		return builder.ToString();
	}

	protected void SetField(FlatFileFieldProperty fieldProperty, ZInt value)
	{
		this.SetField(fieldProperty, value.ToString());
	}

	protected void SetField(FlatFileFieldProperty fieldProperty, ZString value)
	{
		base.SetField(fieldProperty.Name, value.Left(fieldProperty.Length));
	}

	protected void SetField(FlatFileFieldProperty fieldProperty, ZDecimal value)
	{
		base.SetField(fieldProperty.Name, value, fieldProperty.Length);
	}

	protected void SetField(FlatFileFieldProperty fieldProperty, ZDateTime value)
	{
		base.SetField(fieldProperty.Name, value, DateTimeFormat);
	}

	protected ZDateTime GetFieldAsZDateTime(FlatFileFieldProperty fieldProperty)
	{
		return base.GetFieldAsZDateTime(fieldProperty.Name, DateTimeFormat);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Client Specific Format")]
	protected const string DateTimeFormat = "dd-MMM-yyyy"; // Client Specific Format

	protected abstract void SetPreambleData();
	public abstract string RecordIdentifier { get; }

	public ManifestLineCollection Children
	{
		get { return children ?? (children = new ManifestLineCollection()); }
		internal set { children = value; }
	}

	ManifestLineCollection children;
}
