namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.TrackAndTraceNotificationsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class TrackAndTraceNotificationsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TrackAndTraceNotificationsRule>
	{
		public TrackAndTraceNotificationsRegistryDataType()
			: this(null)
		{
		}

		public TrackAndTraceNotificationsRegistryDataType(TrackAndTraceNotificationsRuleVisibilityProvider visibilityProvider)
		{
			this.VisibilityProvider = visibilityProvider;
		}

		protected TrackAndTraceNotificationsRuleVisibilityProvider VisibilityProvider { get; set; }

		protected override TrackAndTraceNotificationsRule DeserialiseCore(byte[] value)
		{
			TrackAndTraceNotificationsRule rule = base.DeserialiseCore(value);
			rule.VisibilityProvider = VisibilityProvider;

			return rule;
		}
	}
}
