namespace Enterprise.Customs.GB.Business
{
	public static class MessagePrettierCss
	{
		public const string CSS = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>";

		public const string ProgressCSS = @"<style>
body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}
tr {
  height: 50px;
  vertical-align: center;
}
.Status {
padding-left: 40px;
padding-right: 10px;
}
.StepProgress-item {
  margin-top: 15px;
}
.StepProgress-item{
  content: '';
  width: 12px;
  height: 12px;
}
.StepProgress-item.is-done {
  font-size: 16px;
  color: green;
  text-align: center;
  font-weight: bold;
}
.StepProgress-item.current{
  font-size: 12px;
  text-align: center;
  color: grey;
}
.StepProgress-item.rejected {
  font-size: 16px;
  color: red;
  text-align: center;
  font-weight: bold;
}
</style>";
	}
}
