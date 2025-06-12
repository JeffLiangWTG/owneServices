using Newtonsoft.Json;

namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  public class BasicMonitoringData
  {
    [JsonProperty(PropertyName = "alarm_source")]
    public string Source { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "hostname")]
    public string HostName { get; set; } = string.Empty;

    [JsonProperty(PropertyName="ctm_active")]
    public bool IsActive { get; set; }
    
    [JsonProperty(PropertyName="ctm_notification_kind")]
    public State State { get; set; }
    
    [JsonProperty(PropertyName="ctm_descr")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName="ctm_object_name")]
    public string ObjectName { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName="ctm_object_ref")]
    public string ObjectReference { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName="ctm_object_type")]
    public string ObjectType { get; set; } = string.Empty;

    [JsonProperty(PropertyName="ctm_shortname")]
    public string ShortName { get; set; } = string.Empty;

    [JsonProperty(PropertyName="ctm_type")]
    public CtmType Type { get; set; }
    
    [JsonProperty(PropertyName="ctm_unique_id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty(PropertyName="time")]
    public long Timestamp { get; set; }

    public void Clone(BasicMonitoringData basicData)
    {
      Source = basicData.Source;
      HostName = basicData.HostName;
      IsActive = basicData.IsActive;
      State = basicData.State;
      Description = basicData.Description;
      ObjectName = basicData.ObjectName;
      ObjectReference = basicData.ObjectReference;
      ObjectType = basicData.ObjectType;
      ShortName = basicData.ShortName;
      Type = basicData.Type;
      Id = basicData.Id;
      Timestamp = basicData.Timestamp;
    }
  }

  public enum State
  {
    New = 0,
    Update = 1,
    End = 2,
  }

  public enum CtmType
  {
    MonitorMessageLifetime = 1,
    MonitorErrorQueue = 2,
    MonitorOutQueueContract = 3,
    MonitorOutQueueNode = 4,
    IncomingSessions = 5,
    OutPortFailureNode = 6,
    OutPortFailureContract = 7,
    OutPortFailureParty = 8,
  }
}
