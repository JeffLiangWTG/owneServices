namespace Enterprise.Customs.AE.Business;

public class RootLine : ManifestLine
{
	public RootLine() : base(0) { }
	protected override void SetPreambleData() { }

	public override string RecordIdentifier { get { return string.Empty; } }
}
