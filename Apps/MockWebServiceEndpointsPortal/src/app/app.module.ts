import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { MessageBoxModule } from './messages/messages.module';
import { ConfigService } from './shared/config.service';
import { ProtocolsComponent } from './messages/protocols/protocols.component';
import { SharedModule } from './shared/shared.module';

export const configFactory = (configService: ConfigService) => {
  return () => configService.loadConfig();
}

@NgModule({
  declarations: [
    AppComponent,
    ProtocolsComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    MessageBoxModule,
    SharedModule
  ],
  providers: [{
    provide: APP_INITIALIZER,
    useFactory: configFactory,
    deps: [ConfigService],
    multi: true
  },
  ConfigService],
  bootstrap: [AppComponent]
})
export class AppModule { }
