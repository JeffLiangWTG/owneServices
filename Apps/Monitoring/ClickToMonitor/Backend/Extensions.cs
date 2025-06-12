using Xware.Xt.ExternalWFService;

namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  public static class Extensions
  {
    public static WFValue? GetValue(this WFObject obj, params string[] keys)
    {
      var result = obj;
      foreach (var key in keys)
      {
        if (result != null && result.ContainsKey(key) && result[key] is var value && value != null)
        {
          result = value as WFObject;
          if (result ==  null)
          {
            return value;
          }
        }
        else
        {
          return null;
        }
      }
      return result;
    }
  }
}
