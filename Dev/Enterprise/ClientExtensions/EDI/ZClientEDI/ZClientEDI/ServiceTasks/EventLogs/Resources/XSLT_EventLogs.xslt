<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:evt="http://schemas.microsoft.com/win/2004/08/events/event"
				exclude-result-prefixes="evt" >
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>

  
  <xsl:template match="/">
    <EDI_Exception_Report>
      <Subject>Event Logs Issues- as "<xsl:value-of select="descendant::evt:Provider/@Name"/>" at Level:<xsl:value-of select="descendant::evt:Level"/></Subject>
      <LoginName>
        <xsl:value-of select="descendant::evt:LoginName"/>
      </LoginName>
      <MachineName>
        <xsl:value-of select="descendant::evt:Computer"/>
      </MachineName>
      <UsersEmailAddress>
        <xsl:value-of select="descendant::evt:UsersEmailAddress"/>
      </UsersEmailAddress>
      <Company>
        <xsl:value-of select="descendant::evt:Company"/>
      </Company>
			<TimeOfException>
				<xsl:value-of select="descendant::evt:TimeCreated/@SystemTime"/>
			</TimeOfException>
      <Source>
        <xsl:value-of select="descendant::evt:Source"/>
        <xsl:text> collected from Retrieved Windows Event Logs Service Task(ELS)</xsl:text>
      </Source>
      <ExceptionMessage>
        <xsl:for-each select="descendant::evt:Message">
          <xsl:choose>
            <xsl:when test="position() = 1">
              <xsl:value-of select="." />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="concat($newline,.)" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </ExceptionMessage>
      <ExceptionDescription>
        <xsl:text>wevtutil Report Detail:&#10;</xsl:text>
        <xsl:apply-templates select="/evt:Event/evt:System" mode="serialize"/>
        <xsl:apply-templates select="/evt:Event/evt:EventData" mode="serialize"/>
      </ExceptionDescription>
      <ExceptionDetails>
        <ExceptionType>
          <xsl:value-of select="descendant::evt:ExceptionType"/>
        </ExceptionType>
        <Message>
          <xsl:for-each select="descendant::evt:Message">
            <xsl:choose>
              <xsl:when test="position() = 1">
                <xsl:value-of select="." />
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="concat($newline,.)" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
        </Message>
        <Source>
          <xsl:value-of select="descendant::evt:Source"/>
        </Source>
        <StackTrace>
          <xsl:for-each select="descendant::evt:Call">
            <xsl:apply-templates select="." />
          </xsl:for-each>
        </StackTrace>
      </ExceptionDetails>
    </EDI_Exception_Report>
  </xsl:template>

  <!-- keep comments -->
  <xsl:template match="comment()">
    <xsl:copy>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>

  <!-- serialize nodes to string &lt; AND &gt;-->
  <xsl:template match="*" mode="serialize">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>&gt;</xsl:text>
    <xsl:apply-templates mode="serialize"/>
    <xsl:text>&lt;/</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>&gt;</xsl:text>
  </xsl:template>
  
  <!-- remove namespace -->
  <xsl:template match="*">
    <!-- remove element prefix -->
    <xsl:element name="{local-name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix -->
        <xsl:attribute name="{local-name()}">
          <xsl:value-of select="."/>
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="text()" mode="serialize">
    <xsl:value-of select="."/>
  </xsl:template>

  <xsl:variable name="newline">
    <xsl:text>&#10;</xsl:text>
  </xsl:variable>

</xsl:stylesheet>